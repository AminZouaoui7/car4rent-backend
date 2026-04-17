namespace Car4rentpg.Models
{
    public class CaptchaSettings
    {
        public string Provider { get; set; } = "Turnstile";
        public string SecretKey { get; set; } = string.Empty;
    }
}