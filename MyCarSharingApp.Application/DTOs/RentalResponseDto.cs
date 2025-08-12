namespace MyCarSharingApp.Application.DTOs
{
    public class RentalResponseDto
    {
        public int RentalId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int CarId { get; set; }
        public DateTime RentalDate { get; set; }
        public DateTime ReturnDate { get; set; }
        public DateTime? ActualReturnDate { get; set; }
    }
}

