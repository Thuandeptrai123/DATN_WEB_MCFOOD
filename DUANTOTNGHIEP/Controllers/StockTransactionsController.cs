using DUANTOTNGHIEP.Data;
using DUANTOTNGHIEP.DTOS.BaseResponses;
using DUANTOTNGHIEP.DTOS.StockTransaction;
using DUANTOTNGHIEP.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace DUANTOTNGHIEP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StockTransactionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StockTransactionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/stocktransactions
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var transactions = await _context.StockTransactions
                .Include(t => t.Ingredient)
                .OrderByDescending(t => t.Date)
                .Select(t => new StockTransactionDto
                {
                    Id = t.Id,
                    IngredientId = t.IngredientId,
                    IngredientName = t.Ingredient.Name,
                    Type = t.Type,
                    Quantity = t.Quantity,
                    Date = t.Date,
                    Note = t.Note
                })
                .ToListAsync();

            return Ok(new BaseResponse<List<StockTransactionDto>>
            {
                ErrorCode = 200,
                Message = "Lấy danh sách giao dịch kho thành công!",
                Data = transactions
            });
        }

        // POST: api/stocktransactions
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateStockTransactionDto dto)
        {
            var ingredient = await _context.Ingredients.FindAsync(dto.IngredientId);
            if (ingredient == null)
            {
                return NotFound(new BaseResponse<object>
                {
                    ErrorCode = 404,
                    Message = "Nguyên liệu không tồn tại!"
                });
            }

            if (dto.Type != "Import" && dto.Type != "Export")
            {
                return BadRequest(new BaseResponse<object>
                {
                    ErrorCode = 400,
                    Message = "Loại giao dịch phải là 'Import' hoặc 'Export'."
                });
            }

            if (dto.Type == "Export" && ingredient.QuantityInStock < dto.Quantity)
            {
                return BadRequest(new BaseResponse<object>
                {
                    ErrorCode = 400,
                    Message = "Không đủ nguyên liệu trong kho để xuất!"
                });
            }

            var transaction = new StockTransaction
            {
                Id = Guid.NewGuid(),
                IngredientId = dto.IngredientId,
                Type = dto.Type,
                Quantity = dto.Quantity,
                Note = dto.Note,
                Date = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                CreatedBy = "System",
                UpdatedBy = "System"
            };

            // Cập nhật tồn kho
            if (dto.Type == "Import")
                ingredient.QuantityInStock += dto.Quantity;
            else
                ingredient.QuantityInStock -= dto.Quantity;

            _context.StockTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            return Ok(new BaseResponse<object>
            {
                ErrorCode = 200,
                Message = "Tạo giao dịch thành công!",
                Data = new { transaction.Id }
            });
        }
    }
}
