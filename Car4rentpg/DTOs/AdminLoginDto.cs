using System.ComponentModel.DataAnnotations;

namespace Car4rentpg.DTOs
{
    public class AdminLoginDto
    {
        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string Email { get; set; } = null!;

        [Required]
        [MinLength(8)]
        [MaxLength(200)]
        public string Password { get; set; } = null!;
    }
}