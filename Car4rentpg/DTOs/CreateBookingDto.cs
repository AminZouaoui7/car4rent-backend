namespace Car4rentpg.DTOs
{
    public class CreateBookingDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Email { get; set; } = null!;
        public int Age { get; set; }

        public string PickupCityId { get; set; } = null!;
        public string? ReturnCityId { get; set; }

        public string VehicleId { get; set; } = null!;

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

        // ===== CODE PROMO =====
        public string? PromoCode { get; set; }

        public string? CaptchaToken { get; set; }
    }
}