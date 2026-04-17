using System.ComponentModel.DataAnnotations;

namespace Car4rentpg.DTOs
{
    public class UpdateLongTermRentalQuoteDto
    {
        [Range(0, double.MaxValue)]
        public decimal? ProposedMonthlyPrice { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? ProposedTotalPrice { get; set; }

        public bool IsQuoteSent { get; set; }
    }
}