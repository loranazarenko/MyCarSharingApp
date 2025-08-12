using MyCarSharingApp.Application.DTOs;
using MyCarSharingApp.Domain.Entities;

namespace MyCarSharingApp.Application.Interfaces
{
    public interface ICarService
    {
        Task<CarResponseDto> AddNewCarAsync(CarRequestDto dto);
        Task<IEnumerable<CarResponseDto>> GetAllCarsAsync(int page = 1, int size = 10);
        Task<CarResponseDto> GetCarByIdAsync(int id);
        Task<CarResponseDto> UpdateCarByIdAsync(int id, CarRequestDto dto);
        Task DeleteCarByIdAsync(int id);
    }
}
