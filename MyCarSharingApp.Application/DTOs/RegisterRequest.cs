using System.ComponentModel.DataAnnotations;

namespace MyCarSharingApp.Application.DTOs
{
    public class RegisterRequest 
    {
        [Required]
        public string Username { get; set; } = "";
        [Required, EmailAddress]
        public string Email { get; set; } = "";
        [Required, StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = ""; 
    }
}
