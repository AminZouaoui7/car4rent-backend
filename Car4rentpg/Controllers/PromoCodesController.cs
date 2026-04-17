using Car4rentpg.DATA;
using Car4rentpg.DTOs;
using Car4rentpg.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Car4rentpg.Controllers
{
    [ApiController]
    [Route("api/promo-codes")]
    public class PromoCodesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PromoCodesController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var promoCodes = await _context.PromoCodes
                    .AsNoTracking()
                    .OrderByDescending(p => p.CreatedAt)
                    .Select(p => new
                    {
                        p.Id,
                        p.Code,
                        p.DiscountPercentage,
                        p.IsActive,
                        p.StartDate,
                        p.EndDate,
                        p.MaxUses,
                        p.UsedCount,
                        p.CreatedAt,
                        p.UpdatedAt
                    })
                    .ToListAsync();

                return Ok(promoCodes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Erreur lors du chargement des codes promo.",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var promoCode = await _context.PromoCodes
                    .AsNoTracking()
                    .Where(p => p.Id == id)
                    .Select(p => new
                    {
                        p.Id,
                        p.Code,
                        p.DiscountPercentage,
                        p.IsActive,
                        p.StartDate,
                        p.EndDate,
                        p.MaxUses,
                        p.UsedCount,
                        p.CreatedAt,
                        p.UpdatedAt
                    })
                    .FirstOrDefaultAsync();

                if (promoCode == null)
                    return NotFound(new { message = "Code promo introuvable." });

                return Ok(promoCode);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Erreur lors du chargement du code promo.",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePromoCodeDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Code))
                    return BadRequest(new { message = "Le code est obligatoire." });

                if (dto.DiscountPercentage <= 0 || dto.DiscountPercentage > 100)
                    return BadRequest(new { message = "Le pourcentage doit être entre 1 et 100." });

                if (dto.StartDate.HasValue && dto.EndDate.HasValue && dto.StartDate > dto.EndDate)
                    return BadRequest(new { message = "La date de début doit être antérieure à la date de fin." });

                if (dto.MaxUses.HasValue && dto.MaxUses <= 0)
                    return BadRequest(new { message = "Le nombre maximum d'utilisations doit être supérieur à 0." });

                var normalizedCode = dto.Code.Trim().ToUpper();

                var exists = await _context.PromoCodes
                    .AnyAsync(p => p.Code.ToUpper() == normalizedCode);

                if (exists)
                    return BadRequest(new { message = "Ce code promo existe déjà." });

                var now = DateTime.UtcNow;

                var promoCode = new PromoCode
                {
                    Code = normalizedCode,
                    DiscountPercentage = dto.DiscountPercentage,
                    IsActive = dto.IsActive,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,
                    MaxUses = dto.MaxUses,
                    UsedCount = 0,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                _context.PromoCodes.Add(promoCode);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Code promo créé avec succès.",
                    promoCode = new
                    {
                        promoCode.Id,
                        promoCode.Code,
                        promoCode.DiscountPercentage,
                        promoCode.IsActive,
                        promoCode.StartDate,
                        promoCode.EndDate,
                        promoCode.MaxUses,
                        promoCode.UsedCount,
                        promoCode.CreatedAt,
                        promoCode.UpdatedAt
                    }
                });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new
                {
                    message = "Erreur base de données lors de la création du code promo.",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Erreur interne lors de la création du code promo.",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdatePromoCodeDto dto)
        {
            try
            {
                var promoCode = await _context.PromoCodes.FindAsync(id);

                if (promoCode == null)
                    return NotFound(new { message = "Code promo introuvable." });

                if (string.IsNullOrWhiteSpace(dto.Code))
                    return BadRequest(new { message = "Le code est obligatoire." });

                if (dto.DiscountPercentage <= 0 || dto.DiscountPercentage > 100)
                    return BadRequest(new { message = "Le pourcentage doit être entre 1 et 100." });

                if (dto.StartDate.HasValue && dto.EndDate.HasValue && dto.StartDate > dto.EndDate)
                    return BadRequest(new { message = "La date de début doit être antérieure à la date de fin." });

                if (dto.MaxUses.HasValue && dto.MaxUses <= 0)
                    return BadRequest(new { message = "Le nombre maximum d'utilisations doit être supérieur à 0." });

                var normalizedCode = dto.Code.Trim().ToUpper();

                var exists = await _context.PromoCodes
                    .AnyAsync(p => p.Id != id && p.Code.ToUpper() == normalizedCode);

                if (exists)
                    return BadRequest(new { message = "Un autre code promo utilise déjà ce code." });

                if (dto.MaxUses.HasValue && promoCode.UsedCount > dto.MaxUses.Value)
                {
                    return BadRequest(new
                    {
                        message = "Le nombre maximum d'utilisations ne peut pas être inférieur au nombre déjà utilisé."
                    });
                }

                promoCode.Code = normalizedCode;
                promoCode.DiscountPercentage = dto.DiscountPercentage;
                promoCode.IsActive = dto.IsActive;
                promoCode.StartDate = dto.StartDate;
                promoCode.EndDate = dto.EndDate;
                promoCode.MaxUses = dto.MaxUses;
                promoCode.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Code promo mis à jour avec succès.",
                    promoCode = new
                    {
                        promoCode.Id,
                        promoCode.Code,
                        promoCode.DiscountPercentage,
                        promoCode.IsActive,
                        promoCode.StartDate,
                        promoCode.EndDate,
                        promoCode.MaxUses,
                        promoCode.UsedCount,
                        promoCode.CreatedAt,
                        promoCode.UpdatedAt
                    }
                });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new
                {
                    message = "Erreur base de données lors de la mise à jour du code promo.",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Erreur interne lors de la mise à jour du code promo.",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var promoCode = await _context.PromoCodes.FindAsync(id);

                if (promoCode == null)
                    return NotFound(new { message = "Code promo introuvable." });

                if (promoCode.UsedCount > 0)
                {
                    return BadRequest(new
                    {
                        message = "Impossible de supprimer un code promo déjà utilisé. Désactive-le à la place."
                    });
                }

                _context.PromoCodes.Remove(promoCode);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Code promo supprimé avec succès." });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new
                {
                    message = "Erreur base de données lors de la suppression du code promo.",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Erreur interne lors de la suppression du code promo.",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}/toggle-status")]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            try
            {
                var promoCode = await _context.PromoCodes.FindAsync(id);

                if (promoCode == null)
                    return NotFound(new { message = "Code promo introuvable." });

                promoCode.IsActive = !promoCode.IsActive;
                promoCode.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = promoCode.IsActive
                        ? "Code promo activé avec succès."
                        : "Code promo désactivé avec succès.",
                    promoCode = new
                    {
                        promoCode.Id,
                        promoCode.Code,
                        promoCode.DiscountPercentage,
                        promoCode.IsActive,
                        promoCode.StartDate,
                        promoCode.EndDate,
                        promoCode.MaxUses,
                        promoCode.UsedCount,
                        promoCode.CreatedAt,
                        promoCode.UpdatedAt
                    }
                });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new
                {
                    message = "Erreur base de données lors du changement de statut.",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Erreur interne lors du changement de statut.",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }
    }
}