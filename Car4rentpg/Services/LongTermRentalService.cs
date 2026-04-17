using Car4rentpg.DATA;
using Car4rentpg.DTOs;
using Car4rentpg.Models;
using Microsoft.EntityFrameworkCore;

namespace Car4rentpg.Services
{
    public class LongTermRentalService
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;
        private readonly PricingService _pricingService;

        public LongTermRentalService(
            AppDbContext context,
            EmailService emailService,
            PricingService pricingService)
        {
            _context = context;
            _emailService = emailService;
            _pricingService = pricingService;
        }

        public async Task<LongTermRentalRequest> CreateAsync(CreateLongTermRentalRequestDto dto)
        {
            if (dto.DurationMonths < 1)
            {
                throw new Exception("La réservation longue durée doit être au minimum de 1 mois.");
            }

            var pickupCity = await _context.Cities
                .FirstOrDefaultAsync(c => c.Id == dto.PickupCityId);

            if (pickupCity == null)
            {
                throw new Exception("La ville de départ est introuvable.");
            }

            Vehicle? vehicle = null;

            if (!string.IsNullOrWhiteSpace(dto.VehicleId))
            {
                vehicle = await _context.Vehicles
                    .Include(v => v.Category)
                    .FirstOrDefaultAsync(v => v.Id == dto.VehicleId);

                if (vehicle == null)
                {
                    throw new Exception("Le véhicule sélectionné est introuvable.");
                }
            }

            var normalizedStartDate = DateTime.SpecifyKind(dto.StartDate.Date, DateTimeKind.Utc);

            var pricingResult = await CalculateLongTermPricingAsync(
                vehicle,
                normalizedStartDate,
                dto.DurationMonths
            );

            var request = new LongTermRentalRequest
            {
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                Phone = dto.Phone.Trim(),
                Email = dto.Email.Trim().ToLower(),
                StartDate = normalizedStartDate,
                DurationMonths = dto.DurationMonths,
                PickupCityId = dto.PickupCityId,
                VehicleId = string.IsNullOrWhiteSpace(dto.VehicleId) ? null : dto.VehicleId,
                Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim(),
                Status = (pricingResult.ProposedMonthlyPrice.HasValue || pricingResult.ProposedTotalPrice.HasValue)
                    ? "Quoted"
                    : "Pending",
                ProposedMonthlyPrice = pricingResult.ProposedMonthlyPrice,
                ProposedTotalPrice = pricingResult.ProposedTotalPrice,
                IsQuoteSent = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.LongTermRentalRequests.Add(request);
            await _context.SaveChangesAsync();

            var createdRequest = await GetByIdAsync(request.Id)
                ?? throw new Exception("Erreur lors de la création de la demande.");

            await _emailService.SendLongTermRentalPendingEmailAsync(
                createdRequest.Email,
                $"{createdRequest.FirstName} {createdRequest.LastName}",
                createdRequest.StartDate,
                createdRequest.DurationMonths,
                createdRequest.PickupCity?.Name ?? "Non renseignée",
                createdRequest.Vehicle != null
                    ? $"{createdRequest.Vehicle.Brand} {createdRequest.Vehicle.Model}"
                    : null,
                createdRequest.Notes
            );

            return createdRequest;
        }

        public async Task<List<LongTermRentalRequest>> GetAllAsync()
        {
            return await _context.LongTermRentalRequests
                .Include(x => x.PickupCity)
                .Include(x => x.Vehicle)
                    .ThenInclude(v => v!.Category)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<LongTermRentalRequest?> GetByIdAsync(string id)
        {
            return await _context.LongTermRentalRequests
                .Include(x => x.PickupCity)
                .Include(x => x.Vehicle)
                    .ThenInclude(v => v!.Category)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<LongTermRentalRequest> UpdateStatusAsync(string id, UpdateLongTermRentalStatusDto dto)
        {
            var request = await _context.LongTermRentalRequests
                .Include(x => x.PickupCity)
                .Include(x => x.Vehicle)
                    .ThenInclude(v => v!.Category)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (request == null)
            {
                throw new Exception("Demande introuvable.");
            }

            var allowedStatuses = new[] { "Pending", "Quoted", "Approved", "Rejected" };

            if (!allowedStatuses.Contains(dto.Status))
            {
                throw new Exception("Statut invalide. Valeurs autorisées: Pending, Quoted, Approved, Rejected.");
            }

            request.Status = dto.Status;

            await _context.SaveChangesAsync();

            if (dto.Status == "Approved")
            {
                await _emailService.SendLongTermRentalApprovedEmailAsync(
                    request.Email,
                    $"{request.FirstName} {request.LastName}",
                    request.StartDate,
                    request.DurationMonths,
                    request.PickupCity?.Name ?? "Non renseignée",
                    request.Vehicle != null
                        ? $"{request.Vehicle.Brand} {request.Vehicle.Model}"
                        : null,
                    request.ProposedMonthlyPrice,
                    request.ProposedTotalPrice
                );
            }
            else if (dto.Status == "Rejected")
            {
                await _emailService.SendLongTermRentalRejectedEmailAsync(
                    request.Email,
                    $"{request.FirstName} {request.LastName}",
                    request.StartDate,
                    request.DurationMonths,
                    request.PickupCity?.Name ?? "Non renseignée",
                    request.Vehicle != null
                        ? $"{request.Vehicle.Brand} {request.Vehicle.Model}"
                        : null
                );
            }

            return request;
        }

        public async Task<LongTermRentalRequest> UpdateQuoteAsync(string id, UpdateLongTermRentalQuoteDto dto)
        {
            var request = await _context.LongTermRentalRequests
                .Include(x => x.PickupCity)
                .Include(x => x.Vehicle)
                    .ThenInclude(v => v!.Category)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (request == null)
            {
                throw new Exception("Demande introuvable.");
            }

            request.ProposedMonthlyPrice = dto.ProposedMonthlyPrice;
            request.ProposedTotalPrice = dto.ProposedTotalPrice;
            request.IsQuoteSent = dto.IsQuoteSent;

            if (dto.ProposedMonthlyPrice.HasValue || dto.ProposedTotalPrice.HasValue)
            {
                request.Status = "Quoted";
            }

            await _context.SaveChangesAsync();

            if (dto.IsQuoteSent)
            {
                await _emailService.SendLongTermRentalQuoteEmailAsync(
                    request.Email,
                    $"{request.FirstName} {request.LastName}",
                    request.StartDate,
                    request.DurationMonths,
                    request.PickupCity?.Name ?? "Non renseignée",
                    request.Vehicle != null
                        ? $"{request.Vehicle.Brand} {request.Vehicle.Model}"
                        : null,
                    request.ProposedMonthlyPrice,
                    request.ProposedTotalPrice
                );
            }

            return request;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var request = await _context.LongTermRentalRequests
                .FirstOrDefaultAsync(x => x.Id == id);

            if (request == null)
            {
                return false;
            }

            _context.LongTermRentalRequests.Remove(request);
            await _context.SaveChangesAsync();

            return true;
        }

        private async Task<LongTermPricingResult> CalculateLongTermPricingAsync(
            Vehicle? vehicle,
            DateTime startDate,
            int durationMonths)
        {
            if (vehicle == null)
            {
                return new LongTermPricingResult();
            }

            var endDate = startDate.AddMonths(durationMonths);

            var hasPricingRule = await _context.PricingRules
                .AsNoTracking()
                .AnyAsync(r =>
                    r.IsActive &&
                    r.StartDate.Date <= endDate.AddDays(-1) &&
                    r.EndDate.Date >= startDate &&
                    (
                        r.VehicleId == vehicle.Id ||
                        (r.VehicleId == null && r.CategoryId == vehicle.CategoryId)
                    ));

            if (hasPricingRule)
            {
                var pricing = await _pricingService.CalculateAsync(vehicle, startDate, endDate);

                var total = Math.Round(Convert.ToDecimal(pricing.TotalPrice), 2);
                var monthly = Math.Round(total / durationMonths, 2);

                return new LongTermPricingResult
                {
                    ProposedMonthlyPrice = monthly,
                    ProposedTotalPrice = total
                };
            }

            var tariff = await _context.TariffSettings
                .AsNoTracking()
                .Where(t => t.VehicleId == vehicle.Id)
                .OrderByDescending(t => t.UpdatedAt)
                .ThenByDescending(t => t.CreatedAt)
                .FirstOrDefaultAsync();

            if (tariff != null)
            {
                var monthly = Math.Round(tariff.Price1Month, 2);
                var total = Math.Round(monthly * durationMonths, 2);

                return new LongTermPricingResult
                {
                    ProposedMonthlyPrice = monthly,
                    ProposedTotalPrice = total
                };
            }

            var fallbackMonthlyPrice = Math.Round(Convert.ToDecimal(vehicle.BasePriceDay * 30), 2);
            var fallbackTotalPrice = Math.Round(fallbackMonthlyPrice * durationMonths, 2);

            return new LongTermPricingResult
            {
                ProposedMonthlyPrice = fallbackMonthlyPrice,
                ProposedTotalPrice = fallbackTotalPrice
            };
        }

        private class LongTermPricingResult
        {
            public decimal? ProposedMonthlyPrice { get; set; }
            public decimal? ProposedTotalPrice { get; set; }
        }
    }
}