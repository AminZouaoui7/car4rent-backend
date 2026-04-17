namespace Car4rentpg.Models
{
    public class PromoCode
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Code { get; set; } = null!;

        public double DiscountPercentage { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public int? MaxUses { get; set; }

        public int UsedCount { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}