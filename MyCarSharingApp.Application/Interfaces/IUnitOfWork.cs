using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCarSharingApp.Application.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        ICarRepository CarRepository { get; }
        IRentalRepository RentalRepository { get; }

        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
        Task<int> SaveChangesAsync();
    }
}
