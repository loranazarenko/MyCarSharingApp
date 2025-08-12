using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCarSharingApp.Application.DTOs
{
    public class RentalWithDetailsDto
    {
        public int RentalId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int CarId { get; set; }

        public DateTime RentalDate { get; set; }
        public DateTime ReturnDate { get; set; }
        public DateTime? ActualReturnDate { get; set; }

        // Car fields
        public string? CarBrand { get; set; }
        public string? CarModel { get; set; }

        // User (Identity) fields - UserName, Email
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
    }
}
