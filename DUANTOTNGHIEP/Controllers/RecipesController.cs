using DUANTOTNGHIEP.Data;
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

        //// GET: api/recipes/food/{foodId}
        //[HttpGet("food/{foodId}")]
        //public async Task<IActionResult> GetByFoodId(Guid foodId)
        //{
        //    var recipes = await _context.Recipes
        //        .Include(r => r.Food)
        //        .Include(r => r.Ingredient)
        //        .Where(r => r.FoodId == foodId)
        //        .Select(r => new RecipeDto
        //        {
        //            FoodId = r.FoodId,
        //            FoodName = r.Food.Name,
        //            IngredientId = r.IngredientId,
        //            IngredientName = r.Ingredient.Name,
        //            QuantityRequired = r.QuantityRequired,
        //            Unit = r.Ingredient.Unit
        //        })
        //        .ToListAsync();

        //    return Ok(recipes);
        //}


        [HttpGet("food/{foodId}")]
        public async Task<IActionResult> GetByFoodId(Guid foodId)
        {
            var food = await _context.Foods.FirstOrDefaultAsync(f => f.Id == foodId);
            if (food == null)
                return NotFound("Food not found");

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

            return Ok(result);
        }


        // POST: api/recipes
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRecipeDto dto)
        {
            var food = await _context.Foods.FindAsync(dto.FoodId);
            var ingredient = await _context.Ingredients.FindAsync(dto.IngredientId);

            if (food == null || ingredient == null)
                return NotFound("Food or Ingredient not found");

            var exists = await _context.Recipes.AnyAsync(r =>
                r.FoodId == dto.FoodId && r.IngredientId == dto.IngredientId);

            if (exists)
                return BadRequest("Ingredient already added to this food");

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

            return Ok(new { message = "Recipe created" });
        }

        // PUT: api/recipes/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRecipeDto dto)
        {
            var recipe = await _context.Recipes.FindAsync(id);
            if (recipe == null)
                return NotFound();

            recipe.QuantityRequired = dto.QuantityRequired;
            recipe.UpdatedDate = DateTime.UtcNow;
            recipe.UpdatedBy = "System";

            await _context.SaveChangesAsync();
            return Ok(new { message = "Recipe updated" });
        }

        // DELETE: api/recipes/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var recipe = await _context.Recipes.FindAsync(id);
            if (recipe == null)
                return NotFound();

            _context.Recipes.Remove(recipe);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Recipe deleted" });
        }


        // POST: api/recipes/bulk
        [HttpPost("bulk")]
        public async Task<IActionResult> BulkCreate([FromBody] BulkCreateRecipeDto dto)
        {
            var food = await _context.Foods.FindAsync(dto.FoodId);
            if (food == null)
                return NotFound("Food not found");

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
            return Ok(new { message = "Recipe ingredients added successfully" });
        }

        // PUT: api/recipes/bulk
        [HttpPut("bulk")]
        public async Task<IActionResult> BulkUpdate([FromBody] BulkUpdateRecipeDto dto)
        {
            var food = await _context.Foods.FindAsync(dto.FoodId);
            if (food == null)
                return NotFound("Food not found");

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
            return Ok(new { message = "Updated all ingredients successfully" });
        }

    }

}
