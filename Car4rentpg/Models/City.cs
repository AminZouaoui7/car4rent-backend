namespace Car4rentpg.Models
{
    public class City
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = null!;
        public string Country { get; set; } = null!;

        // "city" | "airport"
        public string Type { get; set; } = "city";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<Booking> Bookings { get; set; } = new();
    }
}