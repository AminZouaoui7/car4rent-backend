namespace Car4rentpg.DTOs
{
    public class BookingPricePreviewResponseDto
    {
        public double OriginalPrice { get; set; }
        public double DiscountAmount { get; set; }
        public double TotalPrice { get; set; }

        public string? PromoCodeUsed { get; set; }
        public bool PromoApplied { get; set; }

        public double SecondDriverAmount { get; set; }
        public double GpsAmount { get; set; }
        public double FullTankAmount { get; set; }
        public double BoosterSeatAmount { get; set; }
        public double BabySeatAmount { get; set; }
        public double ChildSeatAmount { get; set; }
        public double ProtectionPlusAmount { get; set; }
    }
}