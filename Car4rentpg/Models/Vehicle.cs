namespace Car4rentpg.Models
{
    public class Vehicle
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Brand { get; set; } = null!;
        public string Model { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public double BasePriceDay { get; set; }
        public string Gearbox { get; set; } = null!;
        public string Fuel { get; set; } = null!;
        public int Seats { get; set; }
        public int Bags { get; set; }
        public bool Available { get; set; } = true;
        public string? Image { get; set; }

        public string CategoryId { get; set; } = null!;
        public Category Category { get; set; } = null!;

        public List<Booking> Bookings { get; set; } = new();
        public List<BlackoutPeriod> Blackouts { get; set; } = new();

        // 🔥 Tarifs dédiés à cette voiture
        public List<TariffSettings> TariffSettings { get; set; } = new();

        // 🔥 Pricing rules temporaires qui peuvent écraser le tarif normal
        public List<PricingRule> PricingRules { get; set; } = new();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}