using DUANTOTNGHIEP.Data;
using DUANTOTNGHIEP.DTOS.BaseResponses;
using DUANTOTNGHIEP.DTOS.Recipe;
using DUANTOTNGHIEP.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace DUANTOTNGHIEP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecipesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RecipesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/recipes/food/{foodId}
        [HttpGet("food/{foodId}")]
        public async Task<IActionResult> GetByFoodId(Guid foodId)
        {
            var food = await _context.Foods.FirstOrDefaultAsync(f => f.Id == foodId);
            if (food == null)
                return NotFound(new BaseResponse<object>
                {
                    ErrorCode = 404,
                    Message = "Món ăn không tồn tại!"
                });

            var ingredients = await _context.Recipes
                .Include(r => r.Ingredient)
                .Where(r => r.FoodId == foodId)
                .Select(r => new IngredientInRecipeDto
                {
                    IngredientId = r.IngredientId,
                    IngredientName = r.Ingredient.Name,
                    QuantityRequired = r.QuantityRequired,
                    Unit = r.Ingredient.Unit
                })
                .ToListAsync();

            var result = new FoodRecipeDto
            {
                FoodId = food.Id,
                FoodName = food.Name,
                Ingredients = ingredients
            };

            return Ok(new BaseResponse<FoodRecipeDto>
            {
                ErrorCode = 200,
                Message = "Lấy công thức món ăn thành công!",
                Data = result
            });
        }

        // POST: api/recipes
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRecipeDto dto)
        {
            var food = await _context.Foods.FindAsync(dto.FoodId);
            var ingredient = await _context.Ingredients.FindAsync(dto.IngredientId);

            if (food == null || ingredient == null)
                return NotFound(new BaseResponse<object>
                {
                    ErrorCode = 404,
                    Message = "Món ăn hoặc nguyên liệu không tồn tại!"
                });

            var exists = await _context.Recipes.AnyAsync(r =>
                r.FoodId == dto.FoodId && r.IngredientId == dto.IngredientId);

            if (exists)
                return BadRequest(new BaseResponse<object>
                {
                    ErrorCode = 400,
                    Message = "Nguyên liệu đã tồn tại trong công thức của món ăn này!"
                });

            var recipe = new Recipe
            {
                Id = Guid.NewGuid(),
                FoodId = dto.FoodId,
                IngredientId = dto.IngredientId,
                QuantityRequired = dto.QuantityRequired,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                CreatedBy = "System",
                UpdatedBy = "System"
            };

            _context.Recipes.Add(recipe);
            await _context.SaveChangesAsync();

            return Ok(new BaseResponse<object>
            {
                ErrorCode = 200,
                Message = "Tạo công thức thành công!"
            });
        }

        // PUT: api/recipes/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRecipeDto dto)
        {
            var recipe = await _context.Recipes.FindAsync(id);
            if (recipe == null)
                return NotFound(new BaseResponse<object>
                {
                    ErrorCode = 404,
                    Message = "Công thức không tồn tại!"
                });

            recipe.QuantityRequired = dto.QuantityRequired;
            recipe.UpdatedDate = DateTime.UtcNow;
            recipe.UpdatedBy = "System";

            await _context.SaveChangesAsync();
            return Ok(new BaseResponse<object>
            {
                ErrorCode = 200,
                Message = "Cập nhật công thức thành công!"
            });
        }

        // DELETE: api/recipes/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var recipe = await _context.Recipes.FindAsync(id);
            if (recipe == null)
                return NotFound(new BaseResponse<object>
                {
                    ErrorCode = 404,
                    Message = "Công thức không tồn tại!"
                });

            _context.Recipes.Remove(recipe);
            await _context.SaveChangesAsync();

            return Ok(new BaseResponse<object>
            {
                ErrorCode = 200,
                Message = "Xóa công thức thành công!"
            });
        }

        // POST: api/recipes/bulk
        [HttpPost("bulk")]
        public async Task<IActionResult> BulkCreate([FromBody] BulkCreateRecipeDto dto)
        {
            var food = await _context.Foods.FindAsync(dto.FoodId);
            if (food == null)
                return NotFound(new BaseResponse<object>
                {
                    ErrorCode = 404,
                    Message = "Món ăn không tồn tại!"
                });

            foreach (var item in dto.Ingredients)
            {
                var exists = await _context.Recipes.AnyAsync(r =>
                    r.FoodId == dto.FoodId && r.IngredientId == item.IngredientId);

                if (exists) continue;

                var ingredient = await _context.Ingredients.FindAsync(item.IngredientId);
                if (ingredient == null) continue;

                _context.Recipes.Add(new Recipe
                {
                    Id = Guid.NewGuid(),
                    FoodId = dto.FoodId,
                    IngredientId = item.IngredientId,
                    QuantityRequired = item.QuantityRequired,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow,
                    CreatedBy = "System",
                    UpdatedBy = "System"
                });
            }

            await _context.SaveChangesAsync();
            return Ok(new BaseResponse<object>
            {
                ErrorCode = 200,
                Message = "Thêm nhiều nguyên liệu vào công thức thành công!"
            });
        }

        // PUT: api/recipes/bulk
        [HttpPut("bulk")]
        public async Task<IActionResult> BulkUpdate([FromBody] BulkUpdateRecipeDto dto)
        {
            var food = await _context.Foods.FindAsync(dto.FoodId);
            if (food == null)
                return NotFound(new BaseResponse<object>
                {
                    ErrorCode = 404,
                    Message = "Món ăn không tồn tại!"
                });

            var existingRecipes = await _context.Recipes
                .Where(r => r.FoodId == dto.FoodId)
                .ToListAsync();

            foreach (var item in dto.Ingredients)
            {
                var recipe = existingRecipes
                    .FirstOrDefault(r => r.IngredientId == item.IngredientId);

                if (recipe != null)
                {
                    recipe.QuantityRequired = item.QuantityRequired;
                    recipe.UpdatedDate = DateTime.UtcNow;
                    recipe.UpdatedBy = "System";
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new BaseResponse<object>
            {
                ErrorCode = 200,
                Message = "Cập nhật số lượng nguyên liệu thành công!"
            });
        }

        // DELETE: api/recipes/food/{foodId}/ingredient/{ingredientId}
        [HttpDelete("food/{foodId}/ingredient/{ingredientId}")]
        public async Task<IActionResult> DeleteIngredientFromRecipe(Guid foodId, Guid ingredientId)
        {
            var recipe = await _context.Recipes.FirstOrDefaultAsync(r =>
                r.FoodId == foodId && r.IngredientId == ingredientId);

            if (recipe == null)
                return NotFound(new BaseResponse<object>
                {
                    ErrorCode = 404,
                    Message = "Không tìm thấy nguyên liệu trong công thức!"
                });

            _context.Recipes.Remove(recipe);
            await _context.SaveChangesAsync();

            return Ok(new BaseResponse<object>
            {
                ErrorCode = 200,
                Message = "Xóa nguyên liệu khỏi công thức thành công!"
            });
        }
    }
}
