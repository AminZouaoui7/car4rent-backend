namespace Car4rentpg.Models
{
    public class RefreshToken
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Token { get; set; } = null!;

        public string AdminUserId { get; set; } = null!;
        public AdminUser AdminUser { get; set; } = null!;

        public DateTime ExpiresAtUtc { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public bool IsRevoked { get; set; } = false;
        public DateTime? RevokedAtUtc { get; set; }
        public string? ReplacedByToken { get; set; }

        public bool IsExpired => DateTime.UtcNow >= ExpiresAtUtc;
        public bool IsActive => !IsRevoked && !IsExpired;
    }
}