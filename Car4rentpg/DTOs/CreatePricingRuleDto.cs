namespace Car4rentpg.DTOs
{
    public class CreatePricingRuleDto
    {
        public string? VehicleId { get; set; }
        public string? CategoryId { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public double PricePerDay { get; set; }

        public string? Label { get; set; }

        public bool IsActive { get; set; } = true;
    }
}