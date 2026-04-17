namespace Car4rentpg.Models
{
    public class TariffSettings
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        // 🔥 Lien avec la voiture
        public string VehicleId { get; set; } = null!;
        public Vehicle Vehicle { get; set; } = null!;

        // SEASON ou OFF_SEASON
        public string Type { get; set; } = null!;

        public decimal PriceStart { get; set; }
        public decimal Price3Days { get; set; }
        public decimal Price4To6Days { get; set; }
        public decimal Price7To15Days { get; set; }
        public decimal Price16To29Days { get; set; }
        public decimal Price1Month { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}