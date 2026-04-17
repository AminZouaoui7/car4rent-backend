namespace Car4rentpg.Models
{
    public class Payment
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string BookingId { get; set; } = null!;
        public Booking Booking { get; set; } = null!;

        public string Type { get; set; } = "Deposit";
        public string Status { get; set; } = "Pending";

        public double Amount { get; set; }

        public string? Provider { get; set; }

        public string? TransactionId { get; set; }
        public string? SessionId { get; set; }

        public string? Currency { get; set; } = "EUR";
        public string? Notes { get; set; }
        public string? PaymentUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PaidAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}