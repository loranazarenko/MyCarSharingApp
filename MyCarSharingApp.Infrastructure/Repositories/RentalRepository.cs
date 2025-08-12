using Microsoft.EntityFrameworkCore;
using MyCarSharingApp.Application.DTOs;
using MyCarSharingApp.Application.Interfaces;
using MyCarSharingApp.Domain.Entities;

namespace MyCarSharingApp.Infrastructure.Repositories
{
    public class RentalRepository : IRentalRepository
    {
        private readonly ApplicationDbContext _context;
        public RentalRepository(ApplicationDbContext context) => _context = context;

        public async Task<Rental> AddAsync(Rental rental)
        {
            _context.Rentals.Add(rental);
            await _context.SaveChangesAsync();
            return rental;
        }

        public async Task<Rental?> GetByIdAsync(int id) =>
            await _context.Rentals.FindAsync(id);

        public async Task<bool> UpdateAsync(Rental rental)
        {
            _context.Rentals.Update(rental);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var rental = await _context.Rentals.FindAsync(id);
            if (rental == null) return false;
            _context.Rentals.Remove(rental);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<Rental>> GetByUserIdAsync(string userId, bool? isActive = null)
        {
            var query = _context.Rentals.Where(r => r.UserId == userId);
            if (isActive.HasValue)
            {
                if (isActive.Value)
                    query = query.Where(r => r.ActualReturnDate == null);
                else
                    query = query.Where(r => r.ActualReturnDate != null);
            }
            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Rental>> GetActiveRentalByUserIdAsync(string userId)
        {
            var query = _context.Rentals.Where(r => r.UserId == userId);
            query = query.Where(r => r.ActualReturnDate == null);
            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Rental>> GetAllAsync()
        {
            return await _context.Rentals
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<RentalWithDetailsDto>> GetAllWithDetailsAsync(
            string? userId = null,
            bool? isActive = null,
            int page = 1,
            int pageSize = 50)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 50;

            var usersSet = _context.Users; // 

            var q = from r in _context.Rentals.AsNoTracking()
                    join c in _context.Cars.AsNoTracking() on r.CarId equals c.Id
                    join u in usersSet.AsNoTracking() on r.UserId equals u.Id into ujoin
                    from u in ujoin.DefaultIfEmpty() // left join
                    select new RentalWithDetailsDto
                    {
                        RentalId = r.Id,
                        UserId = r.UserId,
                        CarId = r.CarId,
                        RentalDate = r.RentalDate,
                        ReturnDate = r.ReturnDate,
                        ActualReturnDate = r.ActualReturnDate,
                        CarBrand = c.Brand,
                        CarModel = c.Model,
                        UserName = u != null ? u.UserName : null,
                        UserEmail = u != null ? u.Email : null
                    };

            if (!string.IsNullOrWhiteSpace(userId))
                q = q.Where(x => x.UserId == userId);

            if (isActive.HasValue)
            {
                if (isActive.Value)
                    q = q.Where(x => x.ActualReturnDate == null);
                else
                    q = q.Where(x => x.ActualReturnDate != null);
            }

            var result = await q
                .OrderByDescending(x => x.RentalDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return result;
        }

    }
}
