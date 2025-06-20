using DUANTOTNGHIEP.Data;
using DUANTOTNGHIEP.DTOS.Food;
using DUANTOTNGHIEP.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace DUANTOTNGHIEP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FoodsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public FoodsController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: api/foods
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var foods = await _context.Foods
                .Include(f => f.FoodType)
                .Select(f => new FoodDto
                {
                    Id = f.Id,
                    Name = f.Name,
                    Description = f.Description,
                    Price = f.Price,
                    ImageUrl = f.ImageUrl,
                    FoodTypeId = f.FoodTypeId,
                    FoodTypeName = f.FoodType.FoodTypeName
                }).ToListAsync();

            return Ok(foods);
        }

        // GET: api/foods/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var food = await _context.Foods
                .Include(f => f.FoodType)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (food == null)
                return NotFound();

            var dto = new FoodDto
            {
                Id = food.Id,
                Name = food.Name,
                Description = food.Description,
                Price = food.Price,
                ImageUrl = food.ImageUrl,
                FoodTypeId = food.FoodTypeId,
                FoodTypeName = food.FoodType.FoodTypeName
            };

            return Ok(dto);
        }

        // POST: api/foods
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateFoodDto dto)
        {
            var foodType = await _context.FoodTypes.FindAsync(dto.FoodTypeId);
            if (foodType == null)
                return NotFound("FoodType not found");

            string? imageUrl = null;

            if (dto.ImageFile != null)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.ImageFile.FileName);
                var folderPath = Path.Combine(_env.WebRootPath, "uploads", "food");
                Directory.CreateDirectory(folderPath);
                var filePath = Path.Combine(folderPath, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await dto.ImageFile.CopyToAsync(stream);

                imageUrl = "/uploads/food/" + fileName;
            }


            var food = new Food
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                ImageUrl = imageUrl,
                FoodTypeId = dto.FoodTypeId,
                CreatedBy = "System",
                UpdatedBy = "System",
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            _context.Foods.Add(food);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Created successfully", food.Id });
        }

        // PUT: api/foods/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromForm] UpdateFoodDto dto)
        {
            var food = await _context.Foods.FindAsync(id);
            if (food == null)
                return NotFound();

            food.Name = dto.Name;
            food.Description = dto.Description;
            food.Price = dto.Price;
            food.FoodTypeId = dto.FoodTypeId;
            food.UpdatedBy = "System";
            food.UpdatedDate = DateTime.UtcNow;

            if (dto.ImageFile != null)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.ImageFile.FileName);
                var folderPath = Path.Combine(_env.WebRootPath, "uploads", "food");
                Directory.CreateDirectory(folderPath);
                var filePath = Path.Combine(folderPath, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await dto.ImageFile.CopyToAsync(stream);

                food.ImageUrl = "/uploads/food/" + fileName;
            }


            await _context.SaveChangesAsync();
            return Ok(new { message = "Updated successfully" });
        }

        // DELETE: api/foods/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var food = await _context.Foods.FindAsync(id);
            if (food == null)
                return NotFound();

            _context.Foods.Remove(food);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Deleted successfully" });
        }

    [HttpGet("cookable-quantity/{foodId}")]
        public async Task<IActionResult> GetCookableQuantity(Guid foodId)
        {
            var food = await _context.Foods.FindAsync(foodId);
            if (food == null) return NotFound("Food not found");

            var recipes = await _context.Recipes
                .Include(r => r.Ingredient)
                .Where(r => r.FoodId == foodId)
                .ToListAsync();

            if (!recipes.Any())
                return Ok(new FoodCookableDto
                {
                    FoodId = food.Id,
                    FoodName = food.Name,
                    MaxCookablePortions = 0
                });

            int maxPortions = int.MaxValue;

            foreach (var r in recipes)
            {
                var available = r.Ingredient.QuantityInStock;
                var requiredPerPortion = r.QuantityRequired;

                if (requiredPerPortion == 0) continue;

                int possible = (int)(available / requiredPerPortion);
                maxPortions = Math.Min(maxPortions, possible);
            }

            return Ok(new FoodCookableDto
            {
                FoodId = food.Id,
                FoodName = food.Name,
                MaxCookablePortions = maxPortions
            });
        }
    }
}
