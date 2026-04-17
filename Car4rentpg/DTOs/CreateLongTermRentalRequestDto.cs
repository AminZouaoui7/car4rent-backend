using System.ComponentModel.DataAnnotations;

namespace Car4rentpg.DTOs
{
    public class CreateLongTermRentalRequestDto
    {
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

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        [Range(1, 60, ErrorMessage = "DurationMonths must be at least 1.")]
        public int DurationMonths { get; set; }

        [Required]
        public string PickupCityId { get; set; } = null!;

        public string? VehicleId { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }
}