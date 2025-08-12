using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MyCarSharingApp.Api.Controllers
{
    [ApiController]
    [Route("debug")]
    public class DebugController : ControllerBase
    {
        [HttpGet("claims")]
        [AllowAnonymous] // временно
        public IActionResult TokenInfo()
        {
            var auth = Request.Headers["Authorization"].FirstOrDefault();
            var header = auth ?? "(нет заголовка)";
            var jwtClaims = new List<string>();
            if (!string.IsNullOrEmpty(auth) && auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = auth.Substring("Bearer ".Length).Trim();
                try
                {
                    var jwt = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().ReadJwtToken(token);
                    jwtClaims = jwt.Claims.Select(c => $"{c.Type}:{c.Value}").ToList();
                }
                catch (Exception ex)
                {
                    jwtClaims.Add($"Invalid token: {ex.Message}");
                }
            }

            var principalClaims = User.Claims.Select(c => $"{c.Type}:{c.Value}").ToList();
            var identities = User.Identities.Select(i => new {
                authType = i.AuthenticationType,
                isAuthenticated = i.IsAuthenticated,
                nameClaimType = i.NameClaimType,
                roleClaimType = i.RoleClaimType,
                claims = i.Claims.Select(c => $"{c.Type}:{c.Value}").ToList()
            }).ToList();

            return Ok(new { header, jwtClaims, principalClaims, identities });
        }
    }
}
