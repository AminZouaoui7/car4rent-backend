using Car4rentpg.DATA;
using Car4rentpg.Models;
using Car4rentpg.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Car4rentpg.Controllers
{
    [ApiController]
    [Route("api/vehicles")]
    public class VehiclesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly FileUploadService _fileUploadService;
        private readonly VehicleService _vehicleService;
        private readonly PricingService _pricingService;

        public VehiclesController(
            AppDbContext context,
            FileUploadService fileUploadService,
            VehicleService vehicleService,
            PricingService pricingService)
        {
            _context = context;
            _fileUploadService = fileUploadService;
            _vehicleService = vehicleService;
            _pricingService = pricingService;
        }

        private static string Slugify(string text)
        {
            text = text.ToLowerInvariant().Trim();
            text = Regex.Replace(text, @"[àáâãäå]", "a");
            text = Regex.Replace(text, @"[èéêë]", "e");
            text = Regex.Replace(text, @"[ìíîï]", "i");
            text = Regex.Replace(text, @"[òóôõö]", "o");
            text = Regex.Replace(text, @"[ùúûü]", "u");
            text = Regex.Replace(text, @"[ç]", "c");
            text = Regex.Replace(text, @"[^a-z0-9\s-]", "");
            text = Regex.Replace(text, @"\s+", "-");
            text = Regex.Replace(text, @"-+", "-");
            return text.Trim('-');
        }

        private static string NormalizeTariffType(string? type)
        {
            return (type ?? string.Empty).Trim().ToUpperInvariant();
        }

        private async Task<string> GenerateUniqueSlugAsync(string brand, string model, string? currentVehicleId = null)
        {
            var baseSlug = Slugify($"{brand} {model}");
            var slug = baseSlug;
            var counter = 2;

            while (await _context.Vehicles.AnyAsync(v => v.Slug == slug && v.Id != currentVehicleId))
            {
                slug = $"{baseSlug}-{counter}";
                counter++;
            }

            return slug;
        }

        [HttpGet("available")]
        public async Task<IActionResult> GetAvailable(
    [FromQuery] DateTime startDate,
    [FromQuery] DateTime endDate,
    [FromQuery] string? pricingMode,
    [FromQuery] int durationMonths = 1)
        {
            try
            {
                if (startDate == default || endDate == default)
                    return BadRequest(new { message = "startDate and endDate are required." });

                var start = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc);
                var end = DateTime.SpecifyKind(endDate.Date, DateTimeKind.Utc);

                if (start >= end)
                    return BadRequest(new { message = "startDate must be before endDate." });

                var totalDays = (end - start).Days;

                if (totalDays < 2)
                    return BadRequest(new { message = "Minimum booking duration is 2 days." });

                var vehicles = await _context.Vehicles
                    .Include(v => v.Category)
                    .Where(v => v.Available)
                    .OrderBy(v => v.Brand)
                    .ThenBy(v => v.Model)
                    .ToListAsync();

                var result = new List<VehicleAvailableDto>();

                foreach (var vehicle in vehicles)
                {
                    if (pricingMode == "long-term")
                    {
                        var hasPricingRule = await _context.PricingRules
                            .AsNoTracking()
                            .AnyAsync(r =>
                                r.IsActive &&
                                r.StartDate.Date <= end.AddDays(-1) &&
                                r.EndDate.Date >= start &&
                                (
                                    r.VehicleId == vehicle.Id ||
                                    (r.VehicleId == null && r.CategoryId == vehicle.CategoryId)
                                ));

                        if (hasPricingRule)
                        {
                            var pricingLongTerm = await _pricingService.CalculateAsync(vehicle, start, end);

                            var monthlyPriceFromRule = Math.Round(pricingLongTerm.TotalPrice, 2);
                            var totalLongTermPriceFromRule = Math.Round(monthlyPriceFromRule * durationMonths, 2);

                            result.Add(new VehicleAvailableDto
                            {
                                Id = vehicle.Id,
                                Brand = vehicle.Brand,
                                Model = vehicle.Model,
                                Slug = vehicle.Slug,
                                BasePriceDay = Math.Round(vehicle.BasePriceDay, 2),

                                AppliedPricePerDay = Math.Round(pricingLongTerm.AveragePricePerDay, 2),
                                TotalPrice = Math.Round(pricingLongTerm.TotalPrice, 2),

                                DisplayMonthlyPrice = monthlyPriceFromRule,
                                DisplayTotalPrice = totalLongTermPriceFromRule,

                                AppliedRule = pricingLongTerm.AppliedRule,
                                AppliedSeason = pricingLongTerm.AppliedSeason,
                                HasPricingRule = pricingLongTerm.HasPricingRule,
                                PricingSource = "PRICING_RULE",

                                Gearbox = vehicle.Gearbox,
                                Fuel = vehicle.Fuel,
                                Seats = vehicle.Seats,
                                Bags = vehicle.Bags,
                                Image = vehicle.Image,
                                Category = vehicle.Category == null
                                    ? null
                                    : new CategoryDto
                                    {
                                        Id = vehicle.Category.Id,
                                        Name = vehicle.Category.Name
                                    }
                            });

                            continue;
                        }

                        var currentSeason = await _context.Seasons
                            .AsNoTracking()
                            .FirstOrDefaultAsync(s =>
                                s.IsActive &&
                                start.Date >= s.StartDate.Date &&
                                start.Date <= s.EndDate.Date);

                        var tariffType = currentSeason != null ? "SEASON" : "OFF_SEASON";

                        var tariff = await _context.TariffSettings
                            .AsNoTracking()
                            .Where(t => t.VehicleId == vehicle.Id && t.Type.ToUpper() == tariffType)
                            .OrderByDescending(t => t.UpdatedAt)
                            .ThenByDescending(t => t.CreatedAt)
                            .FirstOrDefaultAsync();

                        if (tariff != null)
                        {
                            var monthlyPriceFromTariff = Math.Round(Convert.ToDouble(tariff.Price1Month) * 30, 2);
                            var totalLongTermPriceFromTariff = Math.Round(monthlyPriceFromTariff * durationMonths, 2);

                            result.Add(new VehicleAvailableDto
                            {
                                Id = vehicle.Id,
                                Brand = vehicle.Brand,
                                Model = vehicle.Model,
                                Slug = vehicle.Slug,
                                BasePriceDay = Math.Round(vehicle.BasePriceDay, 2),

                                AppliedPricePerDay = Math.Round(monthlyPriceFromTariff / 30, 2),
                                TotalPrice = totalLongTermPriceFromTariff,

                                DisplayMonthlyPrice = monthlyPriceFromTariff,
                                DisplayTotalPrice = totalLongTermPriceFromTariff,

                                AppliedRule = null,
                                AppliedSeason = currentSeason != null ? "Saison" : "Hors saison",
                                HasPricingRule = false,
                                PricingSource = "TARIFF",

                                Gearbox = vehicle.Gearbox,
                                Fuel = vehicle.Fuel,
                                Seats = vehicle.Seats,
                                Bags = vehicle.Bags,
                                Image = vehicle.Image,
                                Category = vehicle.Category == null
                                    ? null
                                    : new CategoryDto
                                    {
                                        Id = vehicle.Category.Id,
                                        Name = vehicle.Category.Name
                                    }
                            });

                            continue;
                        }

                        var pricingFallback = await _pricingService.CalculateAsync(vehicle, start, end);
                        var fallbackMonthlyPrice = Math.Round(pricingFallback.AveragePricePerDay * 30, 2);
                        var fallbackTotalPrice = Math.Round(fallbackMonthlyPrice * durationMonths, 2);

                        result.Add(new VehicleAvailableDto
                        {
                            Id = vehicle.Id,
                            Brand = vehicle.Brand,
                            Model = vehicle.Model,
                            Slug = vehicle.Slug,
                            BasePriceDay = Math.Round(vehicle.BasePriceDay, 2),

                            AppliedPricePerDay = Math.Round(pricingFallback.AveragePricePerDay, 2),
                            TotalPrice = fallbackTotalPrice,

                            DisplayMonthlyPrice = fallbackMonthlyPrice,
                            DisplayTotalPrice = fallbackTotalPrice,

                            AppliedRule = null,
                            AppliedSeason = currentSeason != null ? "Saison" : "Hors saison",
                            HasPricingRule = false,
                            PricingSource = "BASE_PRICE",

                            Gearbox = vehicle.Gearbox,
                            Fuel = vehicle.Fuel,
                            Seats = vehicle.Seats,
                            Bags = vehicle.Bags,
                            Image = vehicle.Image,
                            Category = vehicle.Category == null
                                ? null
                                : new CategoryDto
                                {
                                    Id = vehicle.Category.Id,
                                    Name = vehicle.Category.Name
                                }
                        });

                        continue;
                    }

                    var pricingStandard = await _pricingService.CalculateAsync(vehicle, start, end);

                    result.Add(new VehicleAvailableDto
                    {
                        Id = vehicle.Id,
                        Brand = vehicle.Brand,
                        Model = vehicle.Model,
                        Slug = vehicle.Slug,
                        BasePriceDay = Math.Round(vehicle.BasePriceDay, 2),
                        AppliedPricePerDay = pricingStandard.AveragePricePerDay,
                        TotalPrice = pricingStandard.TotalPrice,
                        DisplayMonthlyPrice = null,
                        DisplayTotalPrice = null,
                        AppliedRule = pricingStandard.AppliedRule,
                        AppliedSeason = pricingStandard.AppliedSeason,
                        HasPricingRule = pricingStandard.HasPricingRule,
                        PricingSource = pricingStandard.PricingSource,
                        Gearbox = vehicle.Gearbox,
                        Fuel = vehicle.Fuel,
                        Seats = vehicle.Seats,
                        Bags = vehicle.Bags,
                        Image = vehicle.Image,
                        Category = vehicle.Category == null
                            ? null
                            : new CategoryDto
                            {
                                Id = vehicle.Category.Id,
                                Name = vehicle.Category.Name
                            }
                    });
                }

                return Ok(
                    result
                        .OrderByDescending(v => v.HasPricingRule)
                        .ThenBy(v => pricingMode == "long-term"
                            ? v.DisplayMonthlyPrice ?? double.MaxValue
                            : v.AppliedPricePerDay)
                        .ToList()
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("tarifs")]
        public async Task<IActionResult> GetCurrentTarifs()
        {
            try
            {
                var today = DateTime.UtcNow.Date;

                var currentSeason = await _context.Seasons
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s =>
                        s.IsActive &&
                        today >= s.StartDate.Date &&
                        today <= s.EndDate.Date);

                var isSeasonNow = currentSeason != null;
                var activeType = isSeasonNow ? "SEASON" : "OFF_SEASON";

                var vehicles = await _context.Vehicles
                    .AsNoTracking()
                    .Include(v => v.Category)
                    .Where(v => v.Available)
                    .OrderBy(v => v.Brand)
                    .ThenBy(v => v.Model)
                    .ToListAsync();

                var tariffs = await _context.TariffSettings
                    .AsNoTracking()
                    .OrderByDescending(t => t.UpdatedAt)
                    .ThenByDescending(t => t.CreatedAt)
                    .ToListAsync();

                var result = vehicles.Select(vehicle =>
                {
                    var activeTariff = tariffs.FirstOrDefault(t =>
                        t.VehicleId == vehicle.Id &&
                        NormalizeTariffType(t.Type) == activeType);

                    return new VehicleCurrentTarifDto
                    {
                        Id = vehicle.Id,
                        Brand = vehicle.Brand,
                        Model = vehicle.Model,
                        Slug = vehicle.Slug,
                        Gearbox = vehicle.Gearbox,
                        Fuel = vehicle.Fuel,
                        Seats = vehicle.Seats,
                        Bags = vehicle.Bags,
                        Image = vehicle.Image,
                        BasePriceDay = Math.Round(vehicle.BasePriceDay, 2),
                        Category = vehicle.Category == null
                            ? null
                            : new CategoryDto
                            {
                                Id = vehicle.Category.Id,
                                Name = vehicle.Category.Name
                            },
                        CategoryName = vehicle.Category?.Name,
                        TariffMode = isSeasonNow ? "SAISON" : "HORS SAISON",
                        SeasonName = currentSeason?.Name,
                        CurrentTariff = activeTariff == null
                            ? null
                            : new TariffBlockDto
                            {
                                Type = activeTariff.Type,
                                PriceStart = Convert.ToDouble(activeTariff.PriceStart),
                                Price3Days = Convert.ToDouble(activeTariff.Price3Days),
                                Price4To6Days = Convert.ToDouble(activeTariff.Price4To6Days),
                                Price7To15Days = Convert.ToDouble(activeTariff.Price7To15Days),
                                Price16To29Days = Convert.ToDouble(activeTariff.Price16To29Days),
                                Price1Month = Convert.ToDouble(activeTariff.Price1Month)
                            }
                    };
                }).ToList();

                return Ok(new
                {
                    today,
                    isSeasonNow,
                    currentSeasonName = currentSeason?.Name,
                    tariffMode = isSeasonNow ? "SAISON" : "HORS SAISON",
                    vehicles = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("tarifs/{vehicleId}")]
        public async Task<IActionResult> GetTarifsByVehicle(string vehicleId)
        {
            try
            {
                var vehicle = await _context.Vehicles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(v => v.Id == vehicleId);

                if (vehicle == null)
                    return NotFound(new { message = "Vehicle not found." });

                var tariffs = await _context.TariffSettings
                    .AsNoTracking()
                    .Where(t => t.VehicleId == vehicleId)
                    .OrderByDescending(t => t.UpdatedAt)
                    .ThenByDescending(t => t.CreatedAt)
                    .ToListAsync();

                var seasonTariff = tariffs.FirstOrDefault(t => NormalizeTariffType(t.Type) == "SEASON");
                var offSeasonTariff = tariffs.FirstOrDefault(t => NormalizeTariffType(t.Type) == "OFF_SEASON");

                return Ok(new
                {
                    vehicleId = vehicle.Id,
                    vehicleName = $"{vehicle.Brand} {vehicle.Model}",
                    season = seasonTariff == null ? null : new TariffBlockDto
                    {
                        Type = seasonTariff.Type,
                        PriceStart = Convert.ToDouble(seasonTariff.PriceStart),
                        Price3Days = Convert.ToDouble(seasonTariff.Price3Days),
                        Price4To6Days = Convert.ToDouble(seasonTariff.Price4To6Days),
                        Price7To15Days = Convert.ToDouble(seasonTariff.Price7To15Days),
                        Price16To29Days = Convert.ToDouble(seasonTariff.Price16To29Days),
                        Price1Month = Convert.ToDouble(seasonTariff.Price1Month)
                    },
                    offSeason = offSeasonTariff == null ? null : new TariffBlockDto
                    {
                        Type = offSeasonTariff.Type,
                        PriceStart = Convert.ToDouble(offSeasonTariff.PriceStart),
                        Price3Days = Convert.ToDouble(offSeasonTariff.Price3Days),
                        Price4To6Days = Convert.ToDouble(offSeasonTariff.Price4To6Days),
                        Price7To15Days = Convert.ToDouble(offSeasonTariff.Price7To15Days),
                        Price16To29Days = Convert.ToDouble(offSeasonTariff.Price16To29Days),
                        Price1Month = Convert.ToDouble(offSeasonTariff.Price1Month)
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var vehicles = await _context.Vehicles
                    .AsNoTracking()
                    .Include(v => v.Category)
                    .Where(v => v.Available)
                    .OrderBy(v => v.Brand)
                    .ThenBy(v => v.Model)
                    .Select(v => new
                    {
                        v.Id,
                        v.Brand,
                        v.Model,
                        v.Slug,
                        v.BasePriceDay,
                        v.Gearbox,
                        v.Fuel,
                        v.Seats,
                        v.Bags,
                        v.Image,
                        v.Available,
                        Category = v.Category == null
                            ? null
                            : new
                            {
                                v.Category.Id,
                                v.Category.Name
                            }
                    })
                    .ToListAsync();

                return Ok(vehicles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var v = await _context.Vehicles
                    .Include(v => v.Category)
                    .FirstOrDefaultAsync(v => v.Id == id);

                if (v == null)
                    return NotFound(new { message = "Vehicle not found." });

                return Ok(v);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateVehicleDto dto)
        {
            try
            {
                var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId);
                if (!categoryExists)
                    return BadRequest(new { message = "Category not found." });

                if (dto.BasePriceDay <= 0)
                    return BadRequest(new { message = "BasePriceDay must be greater than 0." });

                if (dto.Seats <= 0)
                    return BadRequest(new { message = "Seats must be greater than 0." });

                if (dto.Bags < 0)
                    return BadRequest(new { message = "Bags cannot be negative." });

                if (string.IsNullOrWhiteSpace(dto.Brand))
                    return BadRequest(new { message = "Brand is required." });

                if (string.IsNullOrWhiteSpace(dto.Model))
                    return BadRequest(new { message = "Model is required." });

                if (string.IsNullOrWhiteSpace(dto.Gearbox))
                    return BadRequest(new { message = "Gearbox is required." });

                if (string.IsNullOrWhiteSpace(dto.Fuel))
                    return BadRequest(new { message = "Fuel is required." });

                var slug = await GenerateUniqueSlugAsync(dto.Brand, dto.Model);

                var vehicle = new Vehicle
                {
                    Id = Guid.NewGuid().ToString(),
                    Brand = dto.Brand.Trim(),
                    Model = dto.Model.Trim(),
                    Slug = slug,
                    BasePriceDay = dto.BasePriceDay,
                    Gearbox = dto.Gearbox.Trim(),
                    Fuel = dto.Fuel.Trim(),
                    Seats = dto.Seats,
                    Bags = dto.Bags,
                    Available = dto.Available,
                    Image = dto.Image?.Trim(),
                    CategoryId = dto.CategoryId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Vehicles.Add(vehicle);
                await _context.SaveChangesAsync();

                return Ok(vehicle);
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new
                {
                    message = "Database update error.",
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Unexpected server error.",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateVehicleDto dto)
        {
            try
            {
                var vehicle = await _context.Vehicles.FindAsync(id);
                if (vehicle == null)
                    return NotFound(new { message = "Vehicle not found." });

                var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId);
                if (!categoryExists)
                    return BadRequest(new { message = "Category not found." });

                if (dto.BasePriceDay <= 0)
                    return BadRequest(new { message = "BasePriceDay must be greater than 0." });

                if (dto.Seats <= 0)
                    return BadRequest(new { message = "Seats must be greater than 0." });

                if (dto.Bags < 0)
                    return BadRequest(new { message = "Bags cannot be negative." });

                if (string.IsNullOrWhiteSpace(dto.Brand))
                    return BadRequest(new { message = "Brand is required." });

                if (string.IsNullOrWhiteSpace(dto.Model))
                    return BadRequest(new { message = "Model is required." });

                if (string.IsNullOrWhiteSpace(dto.Gearbox))
                    return BadRequest(new { message = "Gearbox is required." });

                if (string.IsNullOrWhiteSpace(dto.Fuel))
                    return BadRequest(new { message = "Fuel is required." });

                var oldImage = vehicle.Image;

                vehicle.Brand = dto.Brand.Trim();
                vehicle.Model = dto.Model.Trim();
                vehicle.BasePriceDay = dto.BasePriceDay;
                vehicle.Gearbox = dto.Gearbox.Trim();
                vehicle.Fuel = dto.Fuel.Trim();
                vehicle.Seats = dto.Seats;
                vehicle.Bags = dto.Bags;
                vehicle.Available = dto.Available;
                vehicle.Image = dto.Image?.Trim();
                vehicle.CategoryId = dto.CategoryId;
                vehicle.UpdatedAt = DateTime.UtcNow;

                vehicle.Slug = await GenerateUniqueSlugAsync(dto.Brand, dto.Model, vehicle.Id);

                await _context.SaveChangesAsync();

                if (!string.Equals(oldImage, vehicle.Image, StringComparison.OrdinalIgnoreCase))
                {
                    _fileUploadService.DeleteVehicleImageIfExists(oldImage);
                }

                return Ok(vehicle);
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new
                {
                    message = "Database update error.",
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Unexpected server error.",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}/availability")]
        public async Task<IActionResult> ToggleAvailability(string id, [FromBody] ToggleAvailabilityDto dto)
        {
            try
            {
                var vehicle = await _context.Vehicles.FindAsync(id);
                if (vehicle == null)
                    return NotFound(new { message = "Vehicle not found." });

                vehicle.Available = dto.Available;
                vehicle.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = $"Vehicle availability set to {dto.Available}.",
                    vehicle.Id,
                    vehicle.Available
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var vehicle = await _context.Vehicles
                    .Include(v => v.Bookings)
                    .Include(v => v.TariffSettings)
                    .Include(v => v.PricingRules)
                    .Include(v => v.Blackouts)
                    .FirstOrDefaultAsync(v => v.Id == id);

                if (vehicle == null)
                    return NotFound(new { message = "Vehicle not found." });

                if (vehicle.Bookings != null && vehicle.Bookings.Any())
                {
                    vehicle.Available = false;
                    vehicle.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    return Ok(new
                    {
                        message = "Ce véhicule a des réservations associées. Il a été désactivé au lieu d’être supprimé."
                    });
                }

                var oldImage = vehicle.Image;

                if (vehicle.TariffSettings != null && vehicle.TariffSettings.Any())
                    _context.TariffSettings.RemoveRange(vehicle.TariffSettings);

                if (vehicle.PricingRules != null && vehicle.PricingRules.Any())
                    _context.PricingRules.RemoveRange(vehicle.PricingRules);

                if (vehicle.Blackouts != null && vehicle.Blackouts.Any())
                    _context.BlackoutPeriods.RemoveRange(vehicle.Blackouts);

                _context.Vehicles.Remove(vehicle);
                await _context.SaveChangesAsync();

                _fileUploadService.DeleteVehicleImageIfExists(oldImage);

                return Ok(new { message = "Vehicle deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = "Delete failed.",
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
        }


        [HttpGet("debug-tariffs")]
public async Task<IActionResult> DebugTariffs()
{
    var tariffs = await _context.TariffSettings
        .AsNoTracking()
        .OrderBy(t => t.VehicleId)
        .ThenBy(t => t.Type)
        .Select(t => new
        {
            t.Id,
            t.VehicleId,
            t.Type,
            t.PriceStart,
            t.Price3Days,
            t.Price4To6Days,
            t.Price7To15Days,
            t.Price16To29Days,
            t.Price1Month
        })
        .ToListAsync();

    return Ok(tariffs);
}

        [AllowAnonymous]
        [HttpGet("special-offers")]
        public async Task<IActionResult> GetSpecialOffers()
        {
            try
            {
                var vehicles = await _vehicleService.GetSpecialOfferVehiclesAsync();

                var result = vehicles.Select(v => new
                {
                    v.Id,
                    v.Brand,
                    v.Model,
                    v.Slug,
                    v.BasePriceDay,
                    v.Gearbox,
                    v.Fuel,
                    v.Seats,
                    v.Bags,
                    v.Image,
                    AppliedRule = "Offre spéciale",
                    HasPricingRule = true,
                    PricingSource = "PRICING_RULE",
                    Category = v.Category == null
                        ? null
                        : new
                        {
                            v.Category.Id,
                            v.Category.Name
                        }
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }

    public class VehicleAvailableDto
    {
        public string Id { get; set; } = null!;
        public string Brand { get; set; } = null!;
        public string Model { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public double BasePriceDay { get; set; }
        public double AppliedPricePerDay { get; set; }
        public double TotalPrice { get; set; }
        public string? AppliedRule { get; set; }
        public string? AppliedSeason { get; set; }
        public bool HasPricingRule { get; set; }
        public string PricingSource { get; set; } = null!;
        public string Gearbox { get; set; } = null!;
        public string Fuel { get; set; } = null!;
        public int Seats { get; set; }
        public int Bags { get; set; }
        public string? Image { get; set; }
        public CategoryDto? Category { get; set; }
        public double? DisplayMonthlyPrice { get; set; }
        public double? DisplayTotalPrice { get; set; }
    }

    public class VehicleCurrentTarifDto
    {
        public string Id { get; set; } = null!;
        public string Brand { get; set; } = null!;
        public string Model { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string Gearbox { get; set; } = null!;
        public string Fuel { get; set; } = null!;
        public int Seats { get; set; }
        public int Bags { get; set; }
        public string? Image { get; set; }
        public double BasePriceDay { get; set; }
        public CategoryDto? Category { get; set; }
        public string? CategoryName { get; set; }
        public string TariffMode { get; set; } = null!;
        public string? SeasonName { get; set; }
        public TariffBlockDto? CurrentTariff { get; set; }
    }

    public class TariffBlockDto
    {
        public string Type { get; set; } = null!;
        public double PriceStart { get; set; }
        public double Price3Days { get; set; }
        public double Price4To6Days { get; set; }
        public double Price7To15Days { get; set; }
        public double Price16To29Days { get; set; }
        public double Price1Month { get; set; }
    }

    public class CategoryDto
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
    }

    public class CreateVehicleDto
    {
        public string Brand { get; set; } = null!;
        public string Model { get; set; } = null!;
        public double BasePriceDay { get; set; }
        public string Gearbox { get; set; } = null!;
        public string Fuel { get; set; } = null!;
        public int Seats { get; set; }
        public int Bags { get; set; }
        public bool Available { get; set; } = true;
        public string? Image { get; set; }
        public string CategoryId { get; set; } = null!;
    }

    public class UpdateVehicleDto
    {
        public string Brand { get; set; } = null!;
        public string Model { get; set; } = null!;
        public double BasePriceDay { get; set; }
        public string Gearbox { get; set; } = null!;
        public string Fuel { get; set; } = null!;
        public int Seats { get; set; }
        public int Bags { get; set; }
        public bool Available { get; set; }
        public string? Image { get; set; }
        public string CategoryId { get; set; } = null!;
    }

    public class ToggleAvailabilityDto
    {
        public bool Available { get; set; }
    }
}