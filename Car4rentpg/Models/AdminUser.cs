namespace Car4rentpg.Models
{
    public class AdminUser
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string? Name { get; set; }

        public bool IsActive { get; set; } = true;
        public int FailedLoginAttempts { get; set; } = 0;
        public DateTime? LockoutEndUtc { get; set; }
        public DateTime? LastLoginAtUtc { get; set; }
        public string? LastLoginIp { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<RefreshToken> RefreshTokens { get; set; } = new();
    }
}