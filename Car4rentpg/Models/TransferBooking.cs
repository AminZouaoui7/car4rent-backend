namespace Car4rentpg.Models
{
    public class TransferBooking
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string PickupAirportId { get; set; } = null!;
        public City? PickupAirport { get; set; }

        public string DropoffCityId { get; set; } = null!;
        public City? DropoffCity { get; set; }

        public string HotelName { get; set; } = null!;
        public string? HotelAddress { get; set; }

        public DateTime TransferDate { get; set; }

        public int LuggageCount { get; set; } // ✅ NEW
        public int Passengers { get; set; }

        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Email { get; set; } = null!;

        public TransferBookingStatus Status { get; set; } = TransferBookingStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public enum TransferBookingStatus
    {
        Pending = 1,
        Confirmed = 2,
        Cancelled = 3
    }
}