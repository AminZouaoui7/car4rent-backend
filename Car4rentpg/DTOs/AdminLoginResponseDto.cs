namespace Car4rentpg.DTOs
{
    public class AdminLoginResponseDto
    {
        public string Message { get; set; } = null!;
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public DateTime AccessTokenExpiresAtUtc { get; set; }
    }
}