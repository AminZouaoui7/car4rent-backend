using Car4rentpg.DATA;
using Car4rentpg.DTOs;
using Car4rentpg.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Car4rentpg.Services
{
    public class VehicleService
    {
        private readonly AppDbContext _context;

        public VehicleService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Vehicle> CreateVehicleAsync(CreateVehicleDto dto)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == dto.CategoryId);
            if (category == null)
                throw new Exception("Category not found.");

            if (dto.BasePriceDay <= 0)
                throw new Exception("BasePriceDay must be greater than 0.");

            if (dto.Seats <= 0)
                throw new Exception("Seats must be greater than 0.");

            if (dto.Bags < 0)
                throw new Exception("Bags cannot be negative.");

            if (string.IsNullOrWhiteSpace(dto.Brand))
                throw new Exception("Brand is required.");

            if (string.IsNullOrWhiteSpace(dto.Model))
                throw new Exception("Model is required.");

            if (string.IsNullOrWhiteSpace(dto.Gearbox))
                throw new Exception("Gearbox is required.");

            if (string.IsNullOrWhiteSpace(dto.Fuel))
                throw new Exception("Fuel is required.");

            var baseSlug = GenerateSlug($"{dto.Brand}-{dto.Model}");
            var slug = await GenerateUniqueSlugAsync(baseSlug);

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

            return await _context.Vehicles
                .Include(v => v.Category)
                .FirstAsync(v => v.Id == vehicle.Id);
        }

        public async Task<List<Vehicle>> GetAllVehiclesAsync()
        {
            return await _context.Vehicles
                .Include(v => v.Category)
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Vehicle>> GetAvailableVehiclesAsync(DateTime startDate, DateTime endDate)
        {
            startDate = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc);
            endDate = DateTime.SpecifyKind(endDate.Date, DateTimeKind.Utc);

            if (endDate <= startDate)
                throw new Exception("End date must be after start date.");

            var totalDays = (endDate - startDate).Days;

            if (totalDays < 2)
                throw new Exception("Minimum booking duration is 2 days.");

            // ✅ IMPORTANT :
            // plus aucun blocage selon les réservations existantes
            // on retourne simplement toutes les voitures actives/disponibles
            return await _context.Vehicles
                .Include(v => v.Category)
                .Where(v => v.Available)
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();
        }

        public async Task<Vehicle?> GetVehicleByIdAsync(string id)
        {
            return await _context.Vehicles
                .Include(v => v.Category)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<Vehicle?> UpdateVehicleAsync(string id, UpdateVehicleDto dto)
        {
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == id);
            if (vehicle == null)
                return null;

            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == dto.CategoryId);
            if (category == null)
                throw new Exception("Category not found.");

            if (dto.BasePriceDay <= 0)
                throw new Exception("BasePriceDay must be greater than 0.");

            if (dto.Seats <= 0)
                throw new Exception("Seats must be greater than 0.");

            if (dto.Bags < 0)
                throw new Exception("Bags cannot be negative.");

            if (string.IsNullOrWhiteSpace(dto.Brand))
                throw new Exception("Brand is required.");

            if (string.IsNullOrWhiteSpace(dto.Model))
                throw new Exception("Model is required.");

            if (string.IsNullOrWhiteSpace(dto.Gearbox))
                throw new Exception("Gearbox is required.");

            if (string.IsNullOrWhiteSpace(dto.Fuel))
                throw new Exception("Fuel is required.");

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

            var newBaseSlug = GenerateSlug($"{dto.Brand}-{dto.Model}");
            vehicle.Slug = await GenerateUniqueSlugAsync(newBaseSlug, vehicle.Id);

            await _context.SaveChangesAsync();

            return await _context.Vehicles
                .Include(v => v.Category)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<bool> DeleteVehicleAsync(string id)
        {
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == id);
            if (vehicle == null)
                return false;

            _context.Vehicles.Remove(vehicle);
            await _context.SaveChangesAsync();

            return true;
        }

        private string GenerateSlug(string value)
        {
            value = value.ToLowerInvariant().Trim();

            value = Regex.Replace(value, @"[àáâãäå]", "a");
            value = Regex.Replace(value, @"[èéêë]", "e");
            value = Regex.Replace(value, @"[ìíîï]", "i");
            value = Regex.Replace(value, @"[òóôõö]", "o");
            value = Regex.Replace(value, @"[ùúûü]", "u");
            value = Regex.Replace(value, @"[ç]", "c");

            value = Regex.Replace(value, @"\s+", "-");
            value = Regex.Replace(value, @"[^a-z0-9\-]", "");
            value = Regex.Replace(value, @"-+", "-");

            return value.Trim('-');
        }

        private async Task<string> GenerateUniqueSlugAsync(string baseSlug, string? currentVehicleId = null)
        {
            var slug = baseSlug;
            var counter = 2;

            while (await _context.Vehicles.AnyAsync(v => v.Slug == slug && v.Id != currentVehicleId))
            {
                slug = $"{baseSlug}-{counter}";
                counter++;
            }

            return slug;
        }


        public async Task<List<Vehicle>> GetSpecialOfferVehiclesAsync()
        {
            var today = DateTime.UtcNow.Date;

            var vehicles = await _context.Vehicles
                .AsNoTracking()
                .Include(v => v.Category)
                .Where(v => v.Available)
                .OrderBy(v => v.Brand)
                .ThenBy(v => v.Model)
                .ToListAsync();

            var result = new List<Vehicle>();

            foreach (var vehicle in vehicles)
            {
                var hasActivePricingRule = await _context.PricingRules
                    .AsNoTracking()
                    .AnyAsync(r =>
                        r.IsActive &&
                        r.StartDate.Date <= today &&
                        r.EndDate.Date >= today &&
                        (
                            r.VehicleId == vehicle.Id ||
                            (r.VehicleId == null && r.CategoryId == vehicle.CategoryId)
                        ));

                if (hasActivePricingRule)
                {
                    result.Add(vehicle);
                }
            }

            return result;
        }
    }
}