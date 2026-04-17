using Car4rentpg.DATA;
using Car4rentpg.Models;
using Microsoft.EntityFrameworkCore;

namespace Car4rentpg.Services
{
    public class PricingService
    {
        private readonly AppDbContext _context;

        public PricingService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PricingCalculationResult> CalculateAsync(
            Vehicle vehicle,
            DateTime startDate,
            DateTime endDate)
        {
            startDate = startDate.Date;
            endDate = endDate.Date;

            var totalDays = (endDate - startDate).Days;

            if (totalDays <= 0)
                throw new Exception("Invalid booking dates.");

            var seasons = await _context.Seasons
                .AsNoTracking()
                .Where(s => s.IsActive)
                .ToListAsync();

            var vehicleTariffs = await _context.TariffSettings
                .AsNoTracking()
                .Where(t => t.VehicleId == vehicle.Id)
                .OrderByDescending(t => t.UpdatedAt)
                .ThenByDescending(t => t.CreatedAt)
                .ToListAsync();

            var seasonTariff = vehicleTariffs
                .FirstOrDefault(t => NormalizeTariffType(t.Type) == "SEASON");

            var offSeasonTariff = vehicleTariffs
                .FirstOrDefault(t => NormalizeTariffType(t.Type) == "OFF_SEASON");

            var pricingRules = await _context.PricingRules
                .AsNoTracking()
                .Where(r =>
                    r.IsActive &&
                    r.StartDate.Date <= endDate.AddDays(-1) &&
                    r.EndDate.Date >= startDate &&
                    (
                        r.VehicleId == vehicle.Id ||
                        (r.VehicleId == null && r.CategoryId == vehicle.CategoryId)
                    ))
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var seasonPricePerDay = seasonTariff != null
                ? GetPricePerDayFromTariff(seasonTariff, totalDays)
                : Math.Round(vehicle.BasePriceDay, 2);

            var offSeasonPricePerDay = offSeasonTariff != null
                ? GetPricePerDayFromTariff(offSeasonTariff, totalDays)
                : Math.Round(vehicle.BasePriceDay, 2);

            double totalPrice = 0;
            double pricingRuleTotal = 0;
            double seasonTotal = 0;
            double offSeasonTotal = 0;

            int pricingRuleDays = 0;
            int seasonDays = 0;
            int offSeasonDays = 0;

            string? appliedRule = null;

            for (var current = startDate; current < endDate; current = current.AddDays(1))
            {
                var activeRule = GetBestActiveRuleForDay(pricingRules, vehicle, current);

                if (activeRule != null)
                {
                    var dayPrice = Math.Round(activeRule.PricePerDay, 2);

                    totalPrice += dayPrice;
                    pricingRuleTotal += dayPrice;
                    pricingRuleDays++;

                    appliedRule ??= string.IsNullOrWhiteSpace(activeRule.Label)
                        ? "Offre spéciale"
                        : activeRule.Label;

                    continue;
                }

                var isSeasonDay = seasons.Any(s =>
                    current >= s.StartDate.Date &&
                    current <= s.EndDate.Date);

                if (isSeasonDay)
                {
                    totalPrice += seasonPricePerDay;
                    seasonTotal += seasonPricePerDay;
                    seasonDays++;
                }
                else
                {
                    totalPrice += offSeasonPricePerDay;
                    offSeasonTotal += offSeasonPricePerDay;
                    offSeasonDays++;
                }
            }

            totalPrice = Math.Round(totalPrice, 2);

            string tariffType;
            if (pricingRuleDays > 0 && seasonDays == 0 && offSeasonDays == 0)
                tariffType = "PRICING_RULE";
            else if (pricingRuleDays > 0 || (seasonDays > 0 && offSeasonDays > 0))
                tariffType = "MIXED";
            else if (seasonDays > 0)
                tariffType = "SEASON";
            else
                tariffType = "OFF_SEASON";

            var parts = new List<string>();
            if (pricingRuleDays > 0) parts.Add($"{pricingRuleDays}j pricing rule");
            if (offSeasonDays > 0) parts.Add($"{offSeasonDays}j hors saison");
            if (seasonDays > 0) parts.Add($"{seasonDays}j saison");

            string pricingSource;
            if (pricingRuleDays > 0)
            {
                pricingSource = "PRICING_RULE";
            }
            else if (
                (seasonDays > 0 && seasonTariff != null) ||
                (offSeasonDays > 0 && offSeasonTariff != null))
            {
                pricingSource = "TARIFF";
            }
            else
            {
                pricingSource = "BASE_PRICE";
            }

            return new PricingCalculationResult
            {
                PricingSource = pricingSource,
                TotalPrice = totalPrice,
                AveragePricePerDay = Math.Round(totalPrice / totalDays, 2),
                PricingRuleDays = pricingRuleDays,
                SeasonDays = seasonDays,
                OffSeasonDays = offSeasonDays,
                SeasonPricePerDay = seasonPricePerDay,
                OffSeasonPricePerDay = offSeasonPricePerDay,
                PricingRuleTotal = Math.Round(pricingRuleTotal, 2),
                SeasonTotal = Math.Round(seasonTotal, 2),
                OffSeasonTotal = Math.Round(offSeasonTotal, 2),
                TariffType = tariffType,
                AppliedRule = appliedRule,
                AppliedSeason = parts.Count > 0 ? string.Join(" + ", parts) : null,
                HasPricingRule = pricingRuleDays > 0
            };
        }

        private static PricingRule? GetBestActiveRuleForDay(
            List<PricingRule> pricingRules,
            Vehicle vehicle,
            DateTime currentDay)
        {
            var activeVehicleRule = pricingRules
                .Where(r =>
                    r.VehicleId == vehicle.Id &&
                    currentDay >= r.StartDate.Date &&
                    currentDay <= r.EndDate.Date)
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefault();

            if (activeVehicleRule != null)
                return activeVehicleRule;

            var activeCategoryRule = pricingRules
                .Where(r =>
                    r.VehicleId == null &&
                    r.CategoryId == vehicle.CategoryId &&
                    currentDay >= r.StartDate.Date &&
                    currentDay <= r.EndDate.Date)
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefault();

            return activeCategoryRule;
        }

        private static string NormalizeTariffType(string? type)
        {
            return (type ?? string.Empty).Trim().ToUpperInvariant();
        }

        private static double GetPricePerDayFromTariff(TariffSettings tariff, int totalDays)
        {
            if (totalDays == 3)
                return Convert.ToDouble(tariff.Price3Days);

            if (totalDays >= 4 && totalDays <= 6)
                return Convert.ToDouble(tariff.Price4To6Days);

            if (totalDays >= 7 && totalDays <= 15)
                return Convert.ToDouble(tariff.Price7To15Days);

            if (totalDays >= 16 && totalDays <= 29)
                return Convert.ToDouble(tariff.Price16To29Days);

            if (totalDays >= 30)
                return Convert.ToDouble(tariff.Price1Month);

            return Convert.ToDouble(tariff.PriceStart);
        }
    }

    public class PricingCalculationResult
    {
        public string PricingSource { get; set; } = null!;
        public double TotalPrice { get; set; }
        public double AveragePricePerDay { get; set; }
        public int PricingRuleDays { get; set; }
        public int SeasonDays { get; set; }
        public int OffSeasonDays { get; set; }
        public double SeasonPricePerDay { get; set; }
        public double OffSeasonPricePerDay { get; set; }
        public double PricingRuleTotal { get; set; }
        public double SeasonTotal { get; set; }
        public double OffSeasonTotal { get; set; }
        public string TariffType { get; set; } = null!;
        public string? AppliedRule { get; set; }
        public string? AppliedSeason { get; set; }
        public bool HasPricingRule { get; set; }
    }
}