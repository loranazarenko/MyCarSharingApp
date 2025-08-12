using System.Linq;
using System.Threading.Tasks;
using MyCarSharingApp.Domain.Entities;

namespace MyCarSharingApp.Application.Interfaces
{
    public interface ICarRepository
    {
        IQueryable<Car> Query();

        // CRUD
        Task<Car> AddAsync(Car car);
        Task<Car?> FindByIdAsync(int id);
        Task<Car> UpdateAsync(Car car);
        Task DeleteAsync(int id);
    }
}