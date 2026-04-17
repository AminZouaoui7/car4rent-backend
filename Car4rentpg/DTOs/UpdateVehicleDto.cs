namespace Car4rentpg.DTOs
{
    public class UpdateVehicleDto
    {
        public string Brand { get; set; } = null!;
        public string Model { get; set; } = null!;
        public double BasePriceDay { get; set; }
        public string Gearbox { get; set; } = null!;
        public string Fuel { get; set; } = null!;
        public int Seats { get; set; }
        public int Bags { get; set; }
        public bool Available { get; set; }
        public string? Image { get; set; }
        public string CategoryId { get; set; } = null!;
    }
}