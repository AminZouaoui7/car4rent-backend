using Car4rentpg.DATA;
using Car4rentpg.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Car4rentpg.Controllers
{
    [ApiController]
    [Route("api/cities")]
    public class CitiesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CitiesController(AppDbContext context)
        {
            _context = context;
        }

        // ── GET /api/cities  — PUBLIC ──────────────────────────────────────────
        // Returns all cities ordered by type (cities first, airports second)
        // then alphabetically. Used by the reservation page search form.
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var cities = await _context.Cities
                .OrderBy(c => c.Type)   // "airport" < "city" alphabetically — adjust if needed
                .ThenBy(c => c.Name)
                .Select(c => new CityDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Country = c.Country,
                    Type = c.Type,
                })
                .ToListAsync();

            return Ok(cities);
        }

        // ── GET /api/cities/{id} ───────────────────────────────────────────────
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var city = await _context.Cities.FindAsync(id);
            if (city == null)
                return NotFound(new { message = "City not found." });
            return Ok(city);
        }

        // ── POST /api/cities  (Admin) ──────────────────────────────────────────
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCityDto dto)
        {
            var city = new City
            {
                Id = Guid.NewGuid().ToString(),
                Name = dto.Name.Trim(),
                Country = dto.Country.Trim(),
                Type = dto.Type?.Trim().ToLower() ?? "city",
                CreatedAt = DateTime.UtcNow,
            };

            _context.Cities.Add(city);
            await _context.SaveChangesAsync();
            return Ok(city);
        }

        // ── PUT /api/cities/{id}  (Admin) ──────────────────────────────────────
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] CreateCityDto dto)
        {
            var city = await _context.Cities.FindAsync(id);
            if (city == null)
                return NotFound(new { message = "City not found." });

            city.Name = dto.Name.Trim();
            city.Country = dto.Country.Trim();
            city.Type = dto.Type?.Trim().ToLower() ?? city.Type;

            await _context.SaveChangesAsync();
            return Ok(city);
        }

        // ── DELETE /api/cities/{id}  (Admin) ───────────────────────────────────
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var city = await _context.Cities.FindAsync(id);
            if (city == null)
                return NotFound(new { message = "City not found." });

            _context.Cities.Remove(city);
            await _context.SaveChangesAsync();
            return Ok(new { message = "City deleted successfully." });
        }
    }

    // ── DTOs ──────────────────────────────────────────────────────────────────

    public class CityDto
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Country { get; set; } = null!;
        public string Type { get; set; } = null!;   // "city" | "airport"
    }

    public class CreateCityDto
    {
        public string Name { get; set; } = null!;
        public string Country { get; set; } = null!;
        public string? Type { get; set; }            // "city" | "airport"  (default: "city")
    }
}