using System.ComponentModel.DataAnnotations;

namespace MyCarSharingApp.Application.DTOs
{
    public class RentalRequestDto
    {
        [Required]
        public DateTime RentalDate { get; set; }

        [Required]
        public DateTime ReturnDate { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "CarId must be positive")]
        public int CarId { get; set; }

        [Required]
        public string UserId { get; set; }
    }
}
