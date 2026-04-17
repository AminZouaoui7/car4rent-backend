namespace Car4rentpg.DTOs
{
    public class UpdatePricingRuleDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double PricePerDay { get; set; }
        public string? Label { get; set; }
        public bool IsActive { get; set; }
    }
}