using Car4rentpg.DATA;
using Car4rentpg.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Car4rentpg.Controllers
{
    [ApiController]
    [Route("api/categories")]
    public class CategoriesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CategoriesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _context.Categories
                .OrderBy(c => c.Name)
                .Select(c => new
                {
                    id = c.Id,
                    name = c.Name
                })
                .ToListAsync();

            return Ok(categories);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Category dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
            {
                return BadRequest(new { message = "Le nom de la catégorie est obligatoire." });
            }

            var exists = await _context.Categories
                .AnyAsync(c => c.Name.ToLower() == dto.Name.Trim().ToLower());

            if (exists)
            {
                return BadRequest(new { message = "Cette catégorie existe déjà." });
            }

            var category = new Category
            {
                Id = Guid.NewGuid().ToString(),
                Name = dto.Name.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                id = category.Id,
                name = category.Name
            });
        }
    }
}