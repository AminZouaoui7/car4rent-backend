using Car4rentpg.DATA;
using Car4rentpg.DTOs;
using Car4rentpg.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // ✅ IMPORTANT

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/pricing-rules")]
public class PricingRulesController : ControllerBase
{
    private readonly AppDbContext _context;

    public PricingRulesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var rules = await _context.PricingRules
            .Include(r => r.Vehicle)
            .Include(r => r.Category)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return Ok(rules);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreatePricingRuleDto dto)
    {
        if (dto.VehicleId == null && dto.CategoryId == null)
            return BadRequest(new { message = "VehicleId ou CategoryId est obligatoire." });

        if (dto.VehicleId != null && dto.CategoryId != null)
            return BadRequest(new { message = "Choisir soit VehicleId soit CategoryId, pas les deux." });

        if (dto.EndDate.Date < dto.StartDate.Date)
            return BadRequest(new { message = "La date de fin doit être après la date de début." });

        if (dto.PricePerDay <= 0)
            return BadRequest(new { message = "Le prix par jour doit être supérieur à 0." });

        var startDate = DateTime.SpecifyKind(dto.StartDate.Date, DateTimeKind.Utc);
        var endDate = DateTime.SpecifyKind(dto.EndDate.Date, DateTimeKind.Utc);

        var hasOverlap = await _context.PricingRules.AnyAsync(r =>
            r.IsActive &&
            (
                (dto.VehicleId != null && r.VehicleId == dto.VehicleId) ||
                (dto.CategoryId != null && r.VehicleId == null && r.CategoryId == dto.CategoryId)
            ) &&
            r.StartDate.Date <= endDate.Date &&
            r.EndDate.Date >= startDate.Date
        );

        if (hasOverlap)
        {
            return BadRequest(new
            {
                message = "Une règle active existe déjà sur cette période pour ce véhicule ou cette catégorie."
            });
        }

        var rule = new PricingRule
        {
            VehicleId = dto.VehicleId,
            CategoryId = dto.CategoryId,
            StartDate = startDate,
            EndDate = endDate,
            PricePerDay = dto.PricePerDay,
            Label = dto.Label,
            IsActive = dto.IsActive
        };

        _context.PricingRules.Add(rule);
        await _context.SaveChangesAsync();

        return Ok(rule);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var rule = await _context.PricingRules.FindAsync(id);

        if (rule == null)
        {
            return NotFound(new { message = "Règle introuvable." });
        }

        _context.PricingRules.Remove(rule);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Règle supprimée avec succès." });
    }
}