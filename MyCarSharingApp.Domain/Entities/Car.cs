namespace MyCarSharingApp.Domain.Entities
{
    public class Car
    {
        public int Id { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Inventory { get; set; }
        public bool IsAvailable { get; set; } = true;
        public CarType Type { get; set; } = CarType.Sedan;
    }
    public enum CarType
    {
        Sedan,
        Suv,
        Hatchback,
        Universal
    }
}
