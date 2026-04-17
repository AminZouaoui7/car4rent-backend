namespace Car4rentpg.Models
{
    public class PricingRule
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string? VehicleId { get; set; }
        public Vehicle? Vehicle { get; set; }

        public string? CategoryId { get; set; }
        public Category? Category { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public double PricePerDay { get; set; }

        public bool IsActive { get; set; } = true;

        public string? Label { get; set; } // ex: Haute demande été / Offre spéciale
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}