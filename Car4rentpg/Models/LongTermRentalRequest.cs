using System.ComponentModel.DataAnnotations;

namespace Car4rentpg.Models
{
    public class LongTermRentalRequest
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = null!;

        [Required]
        [MaxLength(30)]
        public string Phone { get; set; } = null!;

        [Required]
        [MaxLength(150)]
        [EmailAddress]
        public string Email { get; set; } = null!;

        public DateTime StartDate { get; set; }

        // Minimum 1 month
        public int DurationMonths { get; set; }

        [Required]
        public string PickupCityId { get; set; } = null!;
        public City? PickupCity { get; set; }

        // Optional: user can request a specific vehicle
        public string? VehicleId { get; set; }
        public Vehicle? Vehicle { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        // Pending / Quoted / Approved / Rejected
        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = "Pending";

        // Optional quote proposed by admin
        public decimal? ProposedMonthlyPrice { get; set; }

        public decimal? ProposedTotalPrice { get; set; }

        public bool IsQuoteSent { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}