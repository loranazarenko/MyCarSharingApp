using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyCarSharingApp.Api.Helpers;
using MyCarSharingApp.Application.DTOs;
using MyCarSharingApp.Application.Interfaces;
using MyCarSharingApp.Application.Services;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService; 
    private readonly JwtTokenGenerator _jwtGen;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IUserService userService, JwtTokenGenerator jwtGen, UserManager<IdentityUser> userManager, 
        ILogger<AuthController> logger)
    {
        _userService = userService;
        _jwtGen = jwtGen;
        _userManager = userManager;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest model)
    {
        var user = await _userService.AuthenticateAsync(model.Email, model.Password); 
        if (user == null) return Unauthorized(new { message = "Invalid credentials" });
        var roles = await _userManager.GetRolesAsync(user);
        _logger.LogInformation($"User {user.Email} roles: {string.Join(", ", roles)}");
        var token = await _jwtGen.GenerateTokenAsync(user);
        return Ok(new { token });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest model)
    {
        var user = await _userService.RegisterAsync(model.Username, model.Email, model.Password);
        if (user == null) return BadRequest(new { message = "Registration failed" });

        await _userManager.AddToRoleAsync(user, "User");

        var token = await _jwtGen.GenerateTokenAsync(user);
        return Ok(new { token });
    }
}