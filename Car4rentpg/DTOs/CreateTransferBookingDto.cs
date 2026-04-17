namespace Car4rentpg.DTOs
{
    public class CreateTransferBookingDto
    {
        public string PickupAirportId { get; set; } = null!;
        public string DropoffCityId { get; set; } = null!;
        public string HotelName { get; set; } = null!;
        public string? HotelAddress { get; set; }

        public DateTime TransferDate { get; set; }

        public int LuggageCount { get; set; } // ✅ NEW
        public int Passengers { get; set; }

        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Email { get; set; } = null!;
    }
}