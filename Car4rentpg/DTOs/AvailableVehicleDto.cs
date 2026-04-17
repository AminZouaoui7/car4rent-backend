namespace Car4rentpg.DTOs
{
    public class AvailableVehicleDto
    {
        public string Id { get; set; } = null!;
        public string Brand { get; set; } = null!;
        public string Model { get; set; } = null!;
        public string Slug { get; set; } = null!;

        public string Category { get; set; } = "";

        public double BasePriceDay { get; set; }

        // ✅ NOUVEAU PRICING
        public double AppliedPricePerDay { get; set; }
        public double TotalPrice { get; set; }
        public string? AppliedRule { get; set; }
        public string? AppliedSeason { get; set; }
        public bool HasPricingRule { get; set; }

        public bool Available { get; set; }
        public string? ImageUrl { get; set; }
        public int? Seats { get; set; }
        public string? Fuel { get; set; }
        public string? Transmission { get; set; }

    }
}