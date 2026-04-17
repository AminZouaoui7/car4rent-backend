namespace Car4rentpg.DTOs
{
    public class UpsertTariffSettingsDto
    {
        public string Type { get; set; } = null!; // SEASON ou OFF_SEASON

        public decimal PriceStart { get; set; }
        public decimal Price3Days { get; set; }
        public decimal Price4To6Days { get; set; }
        public decimal Price7To15Days { get; set; }
        public decimal Price16To29Days { get; set; }
        public decimal Price1Month { get; set; }
    }
}