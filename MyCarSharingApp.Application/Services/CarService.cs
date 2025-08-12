using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyCarSharingApp.Application.DTOs;
using MyCarSharingApp.Application.Exceptions;
using MyCarSharingApp.Application.Interfaces;
using MyCarSharingApp.Application.Mappers;
using MyCarSharingApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MyCarSharingApp.Application.Services
{
    public class CarService : ICarService
    {
        private readonly ICarRepository _repo;
        private readonly ICarMapper _mapper;
        private readonly ILogger<CarService> _logger;

        public CarService(
            ICarRepository repo,
            ICarMapper mapper,
            ILogger<CarService> logger)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<CarResponseDto> AddNewCarAsync(CarRequestDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Brand))
                throw new ArgumentException("Car brand is required.");
            if (string.IsNullOrWhiteSpace(dto.Model))
                throw new ArgumentException("Car model is required.");

            ValidateCarType(dto.Type);

            var entity = _mapper.ToEntity(dto);
            var saved = await _repo.AddAsync(entity);
            _logger.LogInformation("Added car Id={CarId}", saved.Id);
            return _mapper.ToDto(saved);
        }

        public async Task<IEnumerable<CarResponseDto>> GetAllCarsAsync(int page = 1, int size = 10)
        {
            if (page < 1) page = 1;
            if (size < 1) size = 10;

            var query = _repo.Query(); // IQueryable<Car>
            var list = await query
                .AsNoTracking()
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();

            return list.Select(c => _mapper.ToDto(c));
        }

        public async Task<CarResponseDto> GetCarByIdAsync(int id)
        {
            var car = await _repo.FindByIdAsync(id);
            if (car == null)
                throw new EntityNotFoundException($"Can't find a car by this ID: {id}");
            return _mapper.ToDto(car);
        }

        public async Task<CarResponseDto> UpdateCarByIdAsync(int id, CarRequestDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            ValidateCarType(dto.Type);

            var car = await _repo.FindByIdAsync(id);
            if (car == null)
                throw new EntityNotFoundException($"Can't find a car by this ID: {id}");

            _mapper.UpdateEntity(dto, car);
            var updated = await _repo.UpdateAsync(car);
            _logger.LogInformation("Updated car Id={CarId}", id);
            return _mapper.ToDto(updated);
        }

        public async Task DeleteCarByIdAsync(int id)
        {
            var car = await _repo.FindByIdAsync(id);
            if (car == null)
                throw new EntityNotFoundException($"Can't find a car by this ID: {id}");
            await _repo.DeleteAsync(id);
            _logger.LogInformation("Deleted car Id={CarId}", id);
        }

        private static void ValidateCarType(string? type)
        {
            if (string.IsNullOrWhiteSpace(type))
                throw new ArgumentException("Car type is required.");

            if (!Enum.TryParse<CarType>(type, ignoreCase: true, out _))
            {
                var allowed = string.Join(", ", Enum.GetNames(typeof(CarType)));
                throw new ArgumentException($"There is no such type of car: '{type}'. Allowed: {allowed}.");
            }
        }
    }
}