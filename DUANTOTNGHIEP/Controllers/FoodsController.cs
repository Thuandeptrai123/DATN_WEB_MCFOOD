using DUANTOTNGHIEP.Data;
using DUANTOTNGHIEP.DTOS.BaseResponses;
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

            return Ok(new BaseResponse<List<FoodDto>>
            {
                ErrorCode = 200,
                Message = "Lấy danh sách món ăn thành công!",
                Data = foods
            });
        }

        // GET: api/foods/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var food = await _context.Foods
                .Include(f => f.FoodType)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (food == null)
                return NotFound(new BaseResponse<FoodDto>
                {
                    ErrorCode = 404,
                    Message = "Món ăn không tồn tại!"
                });

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

            return Ok(new BaseResponse<FoodDto>
            {
                ErrorCode = 200,
                Message = "Lấy món ăn thành công!",
                Data = dto
            });
        }

        // POST: api/foods
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateFoodDto dto)
        {
            var foodType = await _context.FoodTypes.FindAsync(dto.FoodTypeId);
            if (foodType == null)
                return NotFound(new BaseResponse<object>
                {
                    ErrorCode = 404,
                    Message = "Loại món ăn không tồn tại!"
                });

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

            return Ok(new BaseResponse<object>
            {
                ErrorCode = 200,
                Message = "Tạo món ăn thành công!",
                Data = new { food.Id }
            });
        }

        // PUT: api/foods/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromForm] UpdateFoodDto dto)
        {
            var food = await _context.Foods.FindAsync(id);
            if (food == null)
                return NotFound(new BaseResponse<object>
                {
                    ErrorCode = 404,
                    Message = "Món ăn không tồn tại!"
                });

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

            return Ok(new BaseResponse<object>
            {
                ErrorCode = 200,
                Message = "Cập nhật món ăn thành công!"
            });
        }

        // DELETE: api/foods/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var food = await _context.Foods.FindAsync(id);
            if (food == null)
                return NotFound(new BaseResponse<object>
                {
                    ErrorCode = 404,
                    Message = "Món ăn không tồn tại!"
                });

            _context.Foods.Remove(food);
            await _context.SaveChangesAsync();

            return Ok(new BaseResponse<object>
            {
                ErrorCode = 200,
                Message = "Xóa món ăn thành công!"
            });
        }

        // GET: api/foods/cookable-quantity/{foodId}
        [HttpGet("cookable-quantity/{foodId}")]
        public async Task<IActionResult> GetCookableQuantity(Guid foodId)
        {
            var food = await _context.Foods.FindAsync(foodId);
            if (food == null)
                return NotFound(new BaseResponse<FoodCookableDto>
                {
                    ErrorCode = 404,
                    Message = "Món ăn không tồn tại!"
                });

            var recipes = await _context.Recipes
                .Include(r => r.Ingredient)
                .Where(r => r.FoodId == foodId)
                .ToListAsync();

            if (!recipes.Any())
            {
                return Ok(new BaseResponse<FoodCookableDto>
                {
                    ErrorCode = 200,
                    Message = "Không có công thức nào cho món ăn.",
                    Data = new FoodCookableDto
                    {
                        FoodId = food.Id,
                        FoodName = food.Name,
                        MaxCookablePortions = 0
                    }
                });
            }

            int maxPortions = int.MaxValue;

            foreach (var r in recipes)
            {
                var available = r.Ingredient.QuantityInStock;
                var requiredPerPortion = r.QuantityRequired;

                if (requiredPerPortion == 0) continue;

                int possible = (int)(available / requiredPerPortion);
                maxPortions = Math.Min(maxPortions, possible);
            }

            return Ok(new BaseResponse<FoodCookableDto>
            {
                ErrorCode = 200,
                Message = "Lấy số lượng có thể nấu thành công!",
                Data = new FoodCookableDto
                {
                    FoodId = food.Id,
                    FoodName = food.Name,
                    MaxCookablePortions = maxPortions
                }
            });
        }
    }
}
