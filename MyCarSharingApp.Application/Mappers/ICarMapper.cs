using MyCarSharingApp.Application.DTOs;
using MyCarSharingApp.Domain.Entities;

namespace MyCarSharingApp.Application.Mappers
{
    public interface ICarMapper
    {
        Car ToEntity(CarRequestDto dto);
        CarResponseDto ToDto(Car entity);
        void UpdateEntity(CarRequestDto dto, Car entity);
    }
}