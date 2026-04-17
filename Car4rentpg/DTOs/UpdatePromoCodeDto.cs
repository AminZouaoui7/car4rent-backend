namespace Car4rentpg.DTOs
{
    public class UpdatePromoCodeDto
    {
        public string Code { get; set; } = null!;
        public double DiscountPercentage { get; set; }
        public bool IsActive { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? MaxUses { get; set; }
    }
}