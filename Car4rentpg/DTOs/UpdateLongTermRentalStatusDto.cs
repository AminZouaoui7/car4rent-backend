using System.ComponentModel.DataAnnotations;

namespace Car4rentpg.DTOs
{
    public class UpdateLongTermRentalStatusDto
    {
        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = null!;
    }
}