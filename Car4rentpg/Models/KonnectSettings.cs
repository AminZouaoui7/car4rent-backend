namespace Car4rentpg.Models
{
    public class KonnectSettings
    {
        public string BaseUrl { get; set; } = null!;
        public string ApiKey { get; set; } = null!;
        public string ReceiverWalletId { get; set; } = null!;
        public string Currency { get; set; } = "EUR";
        public double DepositPercent { get; set; } = 10;
        public string WebhookUrl { get; set; } = null!;
        public string SuccessUrl { get; set; } = null!;
        public string CancelUrl { get; set; } = null!;
    }
}