﻿using DUANTOTNGHIEP.Data;
using DUANTOTNGHIEP.DTOS.BaseResponses;
using DUANTOTNGHIEP.DTOS.Ingredient;
using DUANTOTNGHIEP.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace DUANTOTNGHIEP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IngredientsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public IngredientsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/ingredients
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var ingredients = await _context.Ingredients
                .Include(i => i.Provider)
                .Select(i => new IngredientDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    Unit = i.Unit,
                    QuantityInStock = i.QuantityInStock,
                    ProviderId = i.ProviderId,
                    ProviderName = i.Provider.Name
                }).ToListAsync();

            return Ok(new BaseResponse<List<IngredientDto>>
            {
                ErrorCode = 200,
                Message = "Lấy danh sách nguyên liệu thành công!",
                Data = ingredients
            });
        }

        // GET: api/ingredients/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var ingredient = await _context.Ingredients
                .Include(i => i.Provider)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (ingredient == null)
                return NotFound(new BaseResponse<IngredientDto>
                {
                    ErrorCode = 404,
                    Message = "Nguyên liệu không tồn tại!"
                });

            var dto = new IngredientDto
            {
                Id = ingredient.Id,
                Name = ingredient.Name,
                Unit = ingredient.Unit,
                QuantityInStock = ingredient.QuantityInStock,
                ProviderId = ingredient.ProviderId,
                ProviderName = ingredient.Provider.Name
            };

            return Ok(new BaseResponse<IngredientDto>
            {
                ErrorCode = 200,
                Message = "Lấy thông tin nguyên liệu thành công!",
                Data = dto
            });
        }

        // POST: api/ingredients
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateIngredientDto dto)
        {
            var provider = await _context.Providers.FindAsync(dto.ProviderId);
            if (provider == null)
            {
                return NotFound(new BaseResponse<object>
                {
                    ErrorCode = 404,
                    Message = "Nhà cung cấp không tồn tại!"
                });
            }

            var ingredient = new Ingredient
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Unit = dto.Unit,
                QuantityInStock = dto.QuantityInStock,
                ProviderId = dto.ProviderId,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "System",
                UpdatedDate = DateTime.UtcNow,
                UpdatedBy = "System"
            };

            _context.Ingredients.Add(ingredient);
            await _context.SaveChangesAsync();

            return Ok(new BaseResponse<object>
            {
                ErrorCode = 200,
                Message = "Tạo nguyên liệu thành công!",
                Data = new { ingredient.Id }
            });
        }

        // PUT: api/ingredients/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateIngredientDto dto)
        {
            var ingredient = await _context.Ingredients.FindAsync(id);
            if (ingredient == null)
                return NotFound(new BaseResponse<object>
                {
                    ErrorCode = 404,
                    Message = "Nguyên liệu không tồn tại!"
                });

            ingredient.Name = dto.Name;
            ingredient.Unit = dto.Unit;
            ingredient.QuantityInStock = dto.QuantityInStock;
            ingredient.ProviderId = dto.ProviderId;
            ingredient.UpdatedDate = DateTime.UtcNow;
            ingredient.UpdatedBy = "System";

            await _context.SaveChangesAsync();

            return Ok(new BaseResponse<object>
            {
                ErrorCode = 200,
                Message = "Cập nhật nguyên liệu thành công!"
            });
        }

        // DELETE: api/ingredients/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var ingredient = await _context.Ingredients.FindAsync(id);
            if (ingredient == null)
                return NotFound(new BaseResponse<object>
                {
                    ErrorCode = 404,
                    Message = "Nguyên liệu không tồn tại!"
                });

            _context.Ingredients.Remove(ingredient);
            await _context.SaveChangesAsync();

            return Ok(new BaseResponse<object>
            {
                ErrorCode = 200,
                Message = "Xóa nguyên liệu thành công!"
            });
        }
    }
}
