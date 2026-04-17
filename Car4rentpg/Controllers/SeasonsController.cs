using Car4rentpg.DATA;
using Car4rentpg.DTOs;
using Car4rentpg.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Car4rentpg.Controllers
{
    [ApiController]
    [Route("api/seasons")]
    [Authorize(Roles = "Admin")]
    public class SeasonsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SeasonsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var seasons = await _context.Seasons
                    .OrderBy(s => s.StartDate)
                    .ToListAsync();

                return Ok(seasons);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = ex.Message
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var season = await _context.Seasons
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (season == null)
                    return NotFound(new { message = "Season not found." });

                return Ok(season);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = ex.Message
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSeasonDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(new { message = "Season data is required." });

                if (string.IsNullOrWhiteSpace(dto.Name))
                    return BadRequest(new { message = "Season name is required." });

                var startDate = DateTime.SpecifyKind(dto.StartDate.Date, DateTimeKind.Utc);
                var endDate = DateTime.SpecifyKind(dto.EndDate.Date, DateTimeKind.Utc);

                if (startDate > endDate)
                    return BadRequest(new { message = "Start date must be before or equal to end date." });

                var exists = await _context.Seasons.AnyAsync(s =>
                    s.Name.ToLower() == dto.Name.Trim().ToLower());

                if (exists)
                    return BadRequest(new { message = "A season with this name already exists." });

                var overlaps = await _context.Seasons.AnyAsync(s =>
                    startDate <= s.EndDate && endDate >= s.StartDate);

                if (overlaps)
                    return BadRequest(new
                    {
                        message = "This season overlaps with an existing season."
                    });

                var season = new Season
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = dto.Name.Trim(),
                    StartDate = startDate,
                    EndDate = endDate,
                    IsActive = dto.IsActive,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Seasons.Add(season);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Season created successfully.",
                    season
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateSeasonDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(new { message = "Season data is required." });

                var season = await _context.Seasons.FirstOrDefaultAsync(s => s.Id == id);
                if (season == null)
                    return NotFound(new { message = "Season not found." });

                if (string.IsNullOrWhiteSpace(dto.Name))
                    return BadRequest(new { message = "Season name is required." });

                var startDate = DateTime.SpecifyKind(dto.StartDate.Date, DateTimeKind.Utc);
                var endDate = DateTime.SpecifyKind(dto.EndDate.Date, DateTimeKind.Utc);

                if (startDate > endDate)
                    return BadRequest(new { message = "Start date must be before or equal to end date." });

                var exists = await _context.Seasons.AnyAsync(s =>
                    s.Id != id &&
                    s.Name.ToLower() == dto.Name.Trim().ToLower());

                if (exists)
                    return BadRequest(new { message = "A season with this name already exists." });

                var overlaps = await _context.Seasons.AnyAsync(s =>
                    s.Id != id &&
                    startDate <= s.EndDate &&
                    endDate >= s.StartDate);

                if (overlaps)
                    return BadRequest(new
                    {
                        message = "This season overlaps with an existing season."
                    });

                season.Name = dto.Name.Trim();
                season.StartDate = startDate;
                season.EndDate = endDate;
                season.IsActive = dto.IsActive;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Season updated successfully.",
                    season
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var season = await _context.Seasons
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (season == null)
                    return NotFound(new { message = "Season not found." });

                _context.Seasons.Remove(season);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Season deleted successfully."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }
    }
}