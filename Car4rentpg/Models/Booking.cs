namespace Car4rentpg.Models
{
    public class Booking
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Email { get; set; } = null!;
        public int Age { get; set; }

        public string PickupCityId { get; set; } = null!;
        public City PickupCity { get; set; } = null!;

        public string? ReturnCityId { get; set; }
        public City? ReturnCity { get; set; }

        public string VehicleId { get; set; } = null!;
        public Vehicle Vehicle { get; set; } = null!;

        public int? TotalDays { get; set; }

        // ===== OPTIONS =====

        // 2ème conducteur
        public bool HasSecondDriver { get; set; } = false;
        public string? SecondDriverFirstName { get; set; }
        public string? SecondDriverLastName { get; set; }
        public string? SecondDriverPhone { get; set; }
        public double? SecondDriverAmount { get; set; }

        // GPS
        public bool HasGps { get; set; } = false;
        public double? GpsAmount { get; set; }

        // Plein essence
        public bool HasFullTank { get; set; } = false;
        public double? FullTankAmount { get; set; }

        // Rehausseur
        public int BoosterSeatQuantity { get; set; } = 0;
        public double? BoosterSeatAmount { get; set; }

        // Siège bébé (maxi cosi)
        public int BabySeatQuantity { get; set; } = 0;
        public double? BabySeatAmount { get; set; }

        // Siège enfant
        public int ChildSeatQuantity { get; set; } = 0;
        public double? ChildSeatAmount { get; set; }

        // Assurance
        public bool HasProtectionPlus { get; set; } = false;
        public double? ProtectionPlusAmount { get; set; }

        // ===== TARIFICATION =====
        public double? OriginalPrice { get; set; }
        public double? DiscountAmount { get; set; }
        public double? TotalPrice { get; set; }

        // ===== SOURCE DU PRIX =====
        public string? PricingSource { get; set; }
        public string? AppliedRule { get; set; }
        public string? AppliedSeason { get; set; }

        // ===== CODE PROMO =====
        public string? PromoCodeUsed { get; set; }

        public BookingStatus Status { get; set; } = BookingStatus.PENDING;

        // ===== ACCOMPTE / PAIEMENT =====
        public bool IsDepositPaid { get; set; } = false;
        public double? DepositAmount { get; set; }
        public DateTime? DepositPaidAt { get; set; }

        // ===== SOLDE FINAL =====
        public bool IsFullyPaid { get; set; } = false;
        public DateTime? FullyPaidAt { get; set; }

        // ===== NAVIGATION PAYMENTS =====
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}