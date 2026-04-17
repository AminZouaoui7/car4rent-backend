namespace Car4rentpg.Models
{
    public class BlackoutPeriod
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string VehicleId { get; set; } = null!;
        public Vehicle Vehicle { get; set; } = null!;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Reason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}