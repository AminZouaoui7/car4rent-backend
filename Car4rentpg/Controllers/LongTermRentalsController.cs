using Car4rentpg.DTOs;
using Car4rentpg.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Car4rentpg.Controllers
{
    [ApiController]
    [Route("api/long-term-rentals")]
    public class LongTermRentalsController : ControllerBase
    {
        private readonly LongTermRentalService _service;

        public LongTermRentalsController(LongTermRentalService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateLongTermRentalRequestDto dto)
        {
            try
            {
                var result = await _service.CreateAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _service.GetByIdAsync(id);

            if (result == null)
            {
                return NotFound(new { message = "Demande introuvable." });
            }

            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateLongTermRentalStatusDto dto)
        {
            try
            {
                var result = await _service.UpdateStatusAsync(id, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/quote")]
        public async Task<IActionResult> UpdateQuote(string id, [FromBody] UpdateLongTermRentalQuoteDto dto)
        {
            try
            {
                var result = await _service.UpdateQuoteAsync(id, dto);
                return Ok(result);
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
            var deleted = await _service.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound(new { message = "Demande introuvable." });
            }

            return Ok(new { message = "Demande supprimée avec succès." });
        }
    }
}