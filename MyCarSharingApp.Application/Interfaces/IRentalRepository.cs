using MyCarSharingApp.Application.DTOs;
using MyCarSharingApp.Domain.Entities;

namespace MyCarSharingApp.Application.Interfaces
{
    public interface IRentalRepository
    {
        Task<IEnumerable<Rental>> GetAllAsync();
        Task<Rental> AddAsync(Rental rental);
        Task<Rental?> GetByIdAsync(int rentalId);
        Task<bool> UpdateAsync(Rental rental);
        Task<bool> DeleteAsync(int rentalId);
        Task<IEnumerable<Rental>> GetByUserIdAsync(string userId, bool? isActive = null);
        Task<IEnumerable<Rental>> GetActiveRentalByUserIdAsync(string userId);
        Task<IEnumerable<RentalWithDetailsDto>> GetAllWithDetailsAsync(
        string? userId = null,
        bool? isActive = null,
        int page = 1,
        int pageSize = 50);
    }
}
