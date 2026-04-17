namespace Car4rentpg.DTOs
{
    public class BookingPreviewDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string VehicleId { get; set; } = null!;
        public string? PromoCode { get; set; }

        // ===== OPTIONS =====

        // 2ème conducteur
        public bool HasSecondDriver { get; set; } = false;
        public string? SecondDriverFirstName { get; set; }
        public string? SecondDriverLastName { get; set; }
        public string? SecondDriverPhone { get; set; }

        // GPS
        public bool HasGps { get; set; } = false;

        // Plein essence
        public bool HasFullTank { get; set; } = false;

        // Sièges
        public int BoosterSeatQuantity { get; set; } = 0;
        public int BabySeatQuantity { get; set; } = 0;
        public int ChildSeatQuantity { get; set; } = 0;

        // Assurance
        public bool HasProtectionPlus { get; set; } = false;
    }
}