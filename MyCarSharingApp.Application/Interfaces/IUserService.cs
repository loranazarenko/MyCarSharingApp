using Microsoft.AspNetCore.Identity;
using MyCarSharingApp.Domain.Entities;

namespace MyCarSharingApp.Application.Interfaces
{
    public interface IUserService
    {
        Task<IdentityUser> RegisterAsync(string username, string email, string password);
        Task<IdentityUser?> AuthenticateAsync(string email, string password);
    }
}
