using System;
using MyCarSharingApp.Application.DTOs;
using MyCarSharingApp.Application.Mappers;
using MyCarSharingApp.Domain.Entities;

namespace MyCarSharingApp.Infrastructure.EF.Mappers
{
    public class CarMapper : ICarMapper
    {
        public Car ToEntity(CarRequestDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            return new Car
            {
                Brand = dto.Brand ?? string.Empty,
                Model = dto.Model ?? string.Empty,
                Type = ParseTypeOrThrow(dto.Type),
                IsAvailable = true
            };
        }

        public CarResponseDto ToDto(Car entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            return new CarResponseDto
            {
                Id = entity.Id,
                Brand = entity.Brand,
                Model = entity.Model,
                Type = entity.Type.ToString()
            };
        }

        public void UpdateEntity(CarRequestDto dto, Car entity)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            if (!string.IsNullOrWhiteSpace(dto.Brand)) entity.Brand = dto.Brand;
            if (!string.IsNullOrWhiteSpace(dto.Model)) entity.Model = dto.Model;
            if (!string.IsNullOrWhiteSpace(dto.Type)) entity.Type = ParseTypeOrThrow(dto.Type);
        }

        private static CarType ParseTypeOrThrow(string? s)
        {
            if (string.IsNullOrWhiteSpace(s))
                throw new ArgumentException("Car type is required and cannot be empty.");

            if (Enum.TryParse<CarType>(s, ignoreCase: true, out var parsed))
                return parsed;

            var allowed = string.Join(", ", Enum.GetNames(typeof(CarType)));
            throw new ArgumentException($"There is no such type of car: '{s}'. Allowed values: {allowed}.");
        }
    }
}