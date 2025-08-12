using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MyCarSharingApp.Application.Interfaces;

namespace MyCarSharingApp.Application.Services
{
    public class IdentityUserService : IUserService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<IdentityUserService> _logger;

        public IdentityUserService(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ILogger<IdentityUserService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        public async Task<IdentityUser> RegisterAsync(string username, string email, string password)
        {
            var user = new IdentityUser { UserName = username, Email = email };
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                throw new Exception(string.Join(';', result.Errors.Select(e => e.Description)));
            await _userManager.AddToRoleAsync(user, "User");
            return user;
        }

        public async Task<IdentityUser?> AuthenticateAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return null;
            var res = await _signInManager.CheckPasswordSignInAsync(user, password, false);
            return res.Succeeded ? user : null;
        }
    }
}
