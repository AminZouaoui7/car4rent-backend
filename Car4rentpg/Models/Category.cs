using System.Text.Json.Serialization;

namespace Car4rentpg.Models
{
    public class Category
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        public List<Vehicle> Vehicles { get; set; } = new();

    }
}