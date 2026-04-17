using Car4rentpg.DATA;
using Car4rentpg.DTOs;
using Car4rentpg.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Car4rentpg.Controllers
{
    [ApiController]
    [Route("api/tariff-settings")]
    [Authorize(Roles = "Admin")]
    public class TariffSettingsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TariffSettingsController(AppDbContext context)
        {
            _context = context;
        }

        // 🔥 GET ALL (admin table)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tariffs = await _context.TariffSettings
                .Include(t => t.Vehicle)
                .OrderBy(t => t.Vehicle.Brand)
                .ThenBy(t => t.Vehicle.Model)
                .ThenBy(t => t.Type)
                .ToListAsync();

            return Ok(tariffs.Select(t => new
            {
                t.Id,
                t.VehicleId,
                vehicle = $"{t.Vehicle.Brand} {t.Vehicle.Model}",
                t.Type,
                t.PriceStart,
                t.Price3Days,
                t.Price4To6Days,
                t.Price7To15Days,
                t.Price16To29Days,
                t.Price1Month
            }));
        }

        // 🔥 GET TARIFS PAR VOITURE
        [HttpGet("vehicle/{vehicleId}")]
        public async Task<IActionResult> GetByVehicle(string vehicleId)
        {
            var vehicle = await _context.Vehicles.FindAsync(vehicleId);
            if (vehicle == null)
                return NotFound(new { message = "Vehicle not found." });

            var tariffs = await _context.TariffSettings
                .Where(t => t.VehicleId == vehicleId)
                .ToListAsync();

            return Ok(new
            {
                season = tariffs.FirstOrDefault(t => t.Type == "SEASON"),
                offSeason = tariffs.FirstOrDefault(t => t.Type == "OFF_SEASON")
            });
        }

        // 🔥 CREATE / UPDATE TARIF PAR VOITURE
        [HttpPut("vehicle/{vehicleId}/{type}")]
        public async Task<IActionResult> Upsert(
            string vehicleId,
            string type,
            [FromBody] UpsertTariffSettingsDto dto)
        {
            try
            {
                ValidateDto(dto);

                var normalizedType = NormalizeType(type);

                var vehicle = await _context.Vehicles.FindAsync(vehicleId);
                if (vehicle == null)
                    return NotFound(new { message = "Vehicle not found." });

                var tariff = await _context.TariffSettings
                    .FirstOrDefaultAsync(t =>
                        t.VehicleId == vehicleId &&
                        t.Type == normalizedType);

                if (tariff == null)
                {
                    tariff = new TariffSettings
                    {
                        Id = Guid.NewGuid().ToString(),
                        VehicleId = vehicleId,
                        Type = normalizedType,
                        PriceStart = dto.PriceStart,
                        Price3Days = dto.Price3Days,
                        Price4To6Days = dto.Price4To6Days,
                        Price7To15Days = dto.Price7To15Days,
                        Price16To29Days = dto.Price16To29Days,
                        Price1Month = dto.Price1Month,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.TariffSettings.Add(tariff);
                }
                else
                {
                    tariff.PriceStart = dto.PriceStart;
                    tariff.Price3Days = dto.Price3Days;
                    tariff.Price4To6Days = dto.Price4To6Days;
                    tariff.Price7To15Days = dto.Price7To15Days;
                    tariff.Price16To29Days = dto.Price16To29Days;
                    tariff.Price1Month = dto.Price1Month;
                    tariff.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = $"{normalizedType} saved for {vehicle.Brand} {vehicle.Model}",
                    tariff
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // 🔥 DELETE
        [HttpDelete("vehicle/{vehicleId}/{type}")]
        public async Task<IActionResult> Delete(string vehicleId, string type)
        {
            var normalizedType = NormalizeType(type);

            var tariff = await _context.TariffSettings
                .FirstOrDefaultAsync(t =>
                    t.VehicleId == vehicleId &&
                    t.Type == normalizedType);

            if (tariff == null)
                return NotFound(new { message = "Tariff not found." });

            _context.TariffSettings.Remove(tariff);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Deleted successfully." });
        }

        private static string NormalizeType(string type)
        {
            var t = type.Trim().ToUpper();

            if (t != "SEASON" && t != "OFF_SEASON")
                throw new Exception("Type must be SEASON or OFF_SEASON");

            return t;
        }

        private static void ValidateDto(UpsertTariffSettingsDto dto)
        {
            if (dto == null)
                throw new Exception("Data is required.");

            NormalizeType(dto.Type);

            if (dto.PriceStart < 0 ||
                dto.Price3Days < 0 ||
                dto.Price4To6Days < 0 ||
                dto.Price7To15Days < 0 ||
                dto.Price16To29Days < 0 ||
                dto.Price1Month < 0)
            {
                throw new Exception("Prices must be >= 0");
            }
        }
    }
}