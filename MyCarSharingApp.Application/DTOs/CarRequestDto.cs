using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCarSharingApp.Application.DTOs
{
    public class CarRequestDto
    {
        [Required, StringLength(100)]
        public string Brand { get; set; } = string.Empty;
        [Required, StringLength(100)]
        public string Model { get; set; } = string.Empty;
        [Required]
        public string Type { get; set; } = string.Empty; // sedan, suv ect.
        public int Inventory { get; set; }
    }
}
