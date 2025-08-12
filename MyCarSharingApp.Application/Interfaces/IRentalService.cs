using MyCarSharingApp.Domain.Entities;
using MyCarSharingApp.Application.DTOs;

namespace MyCarSharingApp.Application.Interfaces
{
    public interface IRentalService
    {
        Task<RentalResponseDto> RentCarAsync(RentalRequestDto requestDto);
        Task<RentalResponseDto> GetRentalByIdAsync(int rentalId);
        Task<RentalResponseDto> SetActualReturnDateAsync(int rentalId);
        Task<IEnumerable<RentalResponseDto>> GetRentalsByUserIdAsync(string? userId, bool? isActive);
        Task<IEnumerable<RentalWithDetailsDto>> GetRentalsWithDetailsAsync(string? userId, bool? isActive, int page = 1, int pageSize = 50);
    }
}
