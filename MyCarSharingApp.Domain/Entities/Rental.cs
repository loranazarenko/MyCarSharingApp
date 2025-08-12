namespace MyCarSharingApp.Domain.Entities
{
    public class Rental
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public int CarId { get; set; }
        public DateTime RentalDate { get; set; }
        public DateTime ReturnDate { get; set; }
        public DateTime? ActualReturnDate { get; set; }
    }
}
