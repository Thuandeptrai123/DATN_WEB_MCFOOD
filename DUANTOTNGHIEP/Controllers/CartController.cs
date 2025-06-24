using DUANTOTNGHIEP.Data;
using DUANTOTNGHIEP.DTOS.BaseResponses;
using DUANTOTNGHIEP.DTOS.Cart;
using DUANTOTNGHIEP.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DUANTOTNGHIEP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartDTO dto)
        {
            if (dto.FoodId == null && dto.ComboId == null)
            {
                return BadRequest(new BaseResponse<string>
                {
                    ErrorCode = 1,
                    Message = "Phải cung cấp FoodId hoặc ComboId."
                });
            }

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.CustomerId == dto.CustomerId);

            if (cart == null)
            {
                cart = new Cart
                {
                    CartId = Guid.NewGuid(),
                    CustomerId = dto.CustomerId,
                    CreatedDate = DateTime.Now,
                    CreatedBy = "system",
                    UpdatedDate = DateTime.Now,
                    UpdatedBy = "system"
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var item = new CartItem
            {
                CartItemId = Guid.NewGuid(),
                CartId = cart.CartId,
                FoodId = dto.FoodId,
                ComboId = dto.ComboId,
                Quantity = dto.Quantity
            };

            _context.CartItems.Add(item);
            cart.UpdatedDate = DateTime.Now;
            cart.UpdatedBy = "system";

            await _context.SaveChangesAsync();

            return Ok(new BaseResponse<CartItem> { Data = item });
        }

        [HttpGet("{customerId}")]
        public async Task<IActionResult> GetCartByCustomer(Guid customerId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (cart == null)
            {
                return NotFound(new BaseResponse<string>
                {
                    ErrorCode = 404,
                    Message = "Không tìm thấy giỏ hàng."
                });
            }

            return Ok(new BaseResponse<Cart> { Data = cart });
        }

        [HttpDelete("item/{itemId}")]
        public async Task<IActionResult> RemoveItem(Guid itemId)
        {
            var item = await _context.CartItems.FindAsync(itemId);
            if (item == null)
            {
                return NotFound(new BaseResponse<string>
                {
                    ErrorCode = 404,
                    Message = "Không tìm thấy mục trong giỏ hàng."
                });
            }

            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();

            return Ok(new BaseResponse<string>
            {
                Message = "Đã xoá mục khỏi giỏ hàng.",
                Data = $"ItemId: {itemId}"
            });
        }

        [HttpPut("item/{itemId}")]
        public async Task<IActionResult> UpdateQuantity(Guid itemId, [FromBody] int quantity)
        {
            var item = await _context.CartItems.FindAsync(itemId);
            if (item == null)
            {
                return NotFound(new BaseResponse<string>
                {
                    ErrorCode = 404,
                    Message = "Không tìm thấy mục trong giỏ hàng."
                });
            }

            item.Quantity = quantity;
            await _context.SaveChangesAsync();

            return Ok(new BaseResponse<CartItem>
            {
                Data = item,
                Message = "Cập nhật số lượng thành công."
            });
        }
    }
}
