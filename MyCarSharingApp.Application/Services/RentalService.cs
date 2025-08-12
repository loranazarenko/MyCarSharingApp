using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyCarSharingApp.Application.DTOs;
using MyCarSharingApp.Application.Exceptions;
using MyCarSharingApp.Application.Interfaces;
using MyCarSharingApp.Domain.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MyCarSharingApp.Application.Services
{
    public class RentalService : IRentalService
    {
        private readonly ICarRepository _carRepository;
        private readonly IRentalRepository _rentalRepository;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<RentalService> _logger;
        private readonly IUnitOfWork _uow;

        public RentalService(
            ICarRepository carRepository,
            IRentalRepository rentalRepository,
            UserManager<IdentityUser> userManager,
            ILogger<RentalService> logger,
            IUnitOfWork uow)
        {
            _carRepository = carRepository ?? throw new ArgumentNullException(nameof(carRepository));
            _rentalRepository = rentalRepository ?? throw new ArgumentNullException(nameof(rentalRepository));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        /// <summary>
        /// Rents a car for a user (decrements inventory and creates a rental record) within a transaction.
        /// </summary>
        public async Task<RentalResponseDto> RentCarAsync(RentalRequestDto requestDto)
        {
            if (requestDto == null)
                throw new ArgumentNullException(nameof(requestDto));

            await _uow.BeginTransactionAsync();

            try
            {
                // Check that the user exists
                var user = await _userManager.FindByIdAsync(requestDto.UserId);
                if (user == null)
                    throw new EntityNotFoundException($"User with id {requestDto.UserId} not found");

                // Ensure the user has no active rental
                var openRentals = await _rentalRepository.GetActiveRentalByUserIdAsync(requestDto.UserId);
                if (openRentals != null && openRentals.Any())
                    throw new ArgumentException("You already have an open rental.");

                // Find the car and ensure it’s available
                var car = await _carRepository.FindByIdAsync(requestDto.CarId);
                if (car == null)
                    throw new EntityNotFoundException($"Car with id {requestDto.CarId} not found");
                if (car.Inventory <= 0)
                    throw new ArgumentException($"Car id {car.Id} is not available for rent.");

                // Update inventory and save
                car.Inventory -= 1;
                await _carRepository.UpdateAsync(car);

                // Create the rental record
                var rental = new Rental
                {
                    RentalDate = requestDto.RentalDate,
                    ReturnDate = requestDto.ReturnDate,
                    ActualReturnDate = null,
                    UserId = requestDto.UserId,
                    CarId = requestDto.CarId
                };
                await _rentalRepository.AddAsync(rental);

                await _uow.CommitAsync();

                _logger.LogInformation("New rental created: RentalId={RentalId}, User={UserId}, Car={CarId}",
                    rental.Id, requestDto.UserId, requestDto.CarId);

                return new RentalResponseDto
                {
                    RentalId = rental.Id,
                    UserId = rental.UserId,
                    CarId = rental.CarId,
                    RentalDate = rental.RentalDate,
                    ReturnDate = rental.ReturnDate,
                    ActualReturnDate = rental.ActualReturnDate
                };
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync();
                _logger.LogError(ex, "Error renting car: UserId={UserId}, CarId={CarId}", requestDto.UserId, requestDto.CarId);
                throw; // rethrow the exception to be handled by the caller
            }
        }

        public async Task<RentalResponseDto> GetRentalByIdAsync(int rentalId)
        {
            var rental = await _rentalRepository.GetByIdAsync(rentalId)
                         ?? throw new InvalidOperationException($"Can't find a rental by ID: {rentalId}");

            return new RentalResponseDto
            {
                RentalId = rental.Id,
                UserId = rental.UserId,
                CarId = rental.CarId,
                RentalDate = rental.RentalDate,
                ReturnDate = rental.ReturnDate,
                ActualReturnDate = rental.ActualReturnDate
            };
        }

        public async Task<RentalResponseDto> SetActualReturnDateAsync(int rentalId)
        {
            await _uow.BeginTransactionAsync();

            try
            {
                var rental = await _rentalRepository.GetByIdAsync(rentalId)
                         ?? throw new InvalidOperationException($"Can't find a rental by ID: {rentalId}");

            if (rental.ActualReturnDate != null)
                throw new InvalidOperationException("This rental is closed.");

            rental.ActualReturnDate = DateTime.UtcNow;
            await _rentalRepository.UpdateAsync(rental);

            // return car 
            var car = await _carRepository.FindByIdAsync(rental.CarId);
            if (car != null)
            {
                car.Inventory += 1;
                await _carRepository.UpdateAsync(car);
            }

                await _uow.CommitAsync();

                _logger.LogInformation("Rental closed: RentalId={RentalId}", rentalId);

                return new RentalResponseDto
                {
                    RentalId = rental.Id,
                    UserId = rental.UserId,
                    CarId = rental.CarId,
                    RentalDate = rental.RentalDate,
                    ReturnDate = rental.ReturnDate,
                    ActualReturnDate = rental.ActualReturnDate
                };
            }
            catch
            {
                await _uow.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<RentalResponseDto>> GetRentalsByUserIdAsync(string? userId, bool? isActive)
        {
            var rentals = await _rentalRepository.GetAllAsync(); 

            var query = rentals.AsQueryable();

            if (!string.IsNullOrWhiteSpace(userId))
                query = query.Where(r => r.UserId == userId);

            if (isActive.HasValue)
            {
                if (isActive.Value)
                    query = query.Where(r => r.ActualReturnDate == null);
                else
                    query = query.Where(r => r.ActualReturnDate != null);
            }

            return query.Select(r => new RentalResponseDto
            {
                RentalId = r.Id,
                UserId = r.UserId,
                CarId = r.CarId,
                RentalDate = r.RentalDate,
                ReturnDate = r.ReturnDate,
                ActualReturnDate = r.ActualReturnDate
            }).ToList();
        }

        public async Task<IEnumerable<RentalWithDetailsDto>> GetRentalsWithDetailsAsync(string? userId, bool? isActive, int page = 1, int pageSize = 50)
        {
            return await _rentalRepository.GetAllWithDetailsAsync(userId, isActive, page, pageSize);
        }
    }
}
