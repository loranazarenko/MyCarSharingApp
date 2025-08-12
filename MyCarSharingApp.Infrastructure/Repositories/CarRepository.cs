using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyCarSharingApp.Application.Interfaces;
using MyCarSharingApp.Domain.Entities;

namespace MyCarSharingApp.Infrastructure.Repositories
{
    public class CarRepository : ICarRepository
    {
        private readonly ApplicationDbContext _ctx;
        public CarRepository(ApplicationDbContext ctx) => _ctx = ctx;

        public IQueryable<Car> Query() => _ctx.Cars.AsQueryable();

        public async Task<Car> AddAsync(Car car)
        {
            var entry = await _ctx.Cars.AddAsync(car);
            await _ctx.SaveChangesAsync();
            return entry.Entity;
        }

        public async Task<Car?> FindByIdAsync(int id)
        {
            return await _ctx.Cars.FindAsync(id);
        }

        public async Task<Car> UpdateAsync(Car car)
        {
            _ctx.Cars.Update(car);
            await _ctx.SaveChangesAsync();
            return car;
        }

        public async Task DeleteAsync(int id)
        {
            var e = await _ctx.Cars.FindAsync(id);
            if (e != null)
            {
                _ctx.Cars.Remove(e);
                await _ctx.SaveChangesAsync();
            }
        }
    }
}