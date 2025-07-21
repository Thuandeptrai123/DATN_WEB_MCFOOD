using DUANTOTNGHIEP.Data;
using DUANTOTNGHIEP.DTOS.Cart;
using DUANTOTNGHIEP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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

        private Guid GetUserId()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c =>
                c.Type == ClaimTypes.Sid || // Đây là dạng đã được dùng trong login
                c.Type.EndsWith("sid"));    // dự phòng cho kiểu định danh đầy đủ

            if (userIdClaim == null)
                throw new UnauthorizedAccessException("Không tìm thấy userId trong token.");

            return Guid.Parse(userIdClaim.Value);
        }

        // GET: api/cart/user-cart
        [HttpGet("user-cart")]
        [Authorize]
        public async Task<IActionResult> GetCartByUserId()
        {
            try
            {
                var userId = GetUserId(); // Lấy userId từ token

                if (userId == Guid.Empty)
                {
                    return Unauthorized(new { message = "Người dùng chưa đăng nhập." });
                }

                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null)
                {
                    cart = new Cart
                    {
                        CartId = Guid.NewGuid(),
                        UserId = userId,
                        CreatedDate = DateTime.UtcNow
                    };
                    _context.Carts.Add(cart);
                    await _context.SaveChangesAsync();
                }

                var cartItems = await _context.CartItems
                    .Where(ci => ci.CartId == cart.CartId)
                    .ToListAsync();

                var result = new List<object>();

                foreach (var item in cartItems)
                {
                    var foodDetail = item.FoodID != null
                        ? await _context.Foods
                            .Where(f => f.Id == item.FoodID)
                            .Select(f => new
                            {
                                f.Name,
                                f.Description,
                                f.ImageUrl,
                                Quantity = item.Quantity,
                                Price = item.Price
                            })
                            .FirstOrDefaultAsync()
                        : null;

                    var comboDetail = item.ComboID != null
                        ? await _context.Combos
                            .Where(c => c.Id == item.ComboID)
                            .Select(c => new
                            {
                                c.Name,
                                c.Description,
                                c.ImageUrl,
                                Quantity = item.Quantity,
                                Price = item.Price
                            })
                            .FirstOrDefaultAsync()
                        : null;

                    var totalQuantity = item.Quantity;
                    var totalPrice = item.Price * item.Quantity;

                    result.Add(new
                    {
                        item.CartItemId,
                        item.ProductName,
                        TotalQuantity = totalQuantity,
                        TotalPrice = totalPrice,
                        ProductType = item.FoodID != null ? "Food" : "Combo",
                        ProductId = item.FoodID ?? item.ComboID,
                        FoodDetails = foodDetail,
                        ComboDetails = comboDetail
                    });
                }

                var response = new
                {
                    CartId = cart.CartId,
                    TotalItems = result.Count,
                    TotalAmount = result.Sum(i => (decimal)i.GetType().GetProperty("TotalPrice")?.GetValue(i)),
                    Items = result
                };

                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = "Không có quyền truy cập", error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy giỏ hàng người dùng", error = ex.Message });
            }
        }



        [HttpPost("add-item")]
        public async Task<IActionResult> AddItemToCart([FromBody] CartItemDTO request)
        {
            try
            {
                if (request == null)
                    return BadRequest("Invalid request data");

                var userId = GetUserId(); // Lấy từ token

                // Lấy hoặc tạo mới cart
                var cart = await _context.Carts
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null)
                {
                    var userName = User.Identity?.Name ?? "unknown"; // Lấy tên người dùng từ token

                    cart = new Cart
                    {
                        CartId = Guid.NewGuid(),
                        UserId = userId,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = userName,
                        UpdatedDate = DateTime.UtcNow,
                        UpdatedBy = userName
                    };
                    _context.Carts.Add(cart);
                    await _context.SaveChangesAsync();
                }


                List<object> addedItems = new();

                // Nếu có FoodID
                if (request.FoodID.HasValue)
                {
                    var food = await _context.Foods.FindAsync(request.FoodID.Value);
                    if (food == null) return NotFound("Món ăn không tồn tại.");

                    var existingFoodItem = await _context.CartItems.FirstOrDefaultAsync(ci =>
                        ci.CartId == cart.CartId && ci.FoodID == request.FoodID && ci.ComboID == null);

                    if (existingFoodItem == null)
                    {
                        var newItem = new CartItem
                        {
                            CartItemId = Guid.NewGuid(),
                            CartId = cart.CartId,
                            FoodID = request.FoodID,
                            ProductName = food.Name,
                            Price = food.Price,
                            Quantity = request.Quantity
                        };
                        _context.CartItems.Add(newItem);
                        addedItems.Add(new { product = "food", item = newItem });
                    }
                    else
                    {
                        existingFoodItem.Quantity += request.Quantity;
                        existingFoodItem.Price = food.Price;
                        addedItems.Add(new { product = "food", item = existingFoodItem });
                    }
                }

                // Nếu có ComboID
                if (request.ComboID.HasValue)
                {
                    var combo = await _context.Combos.FindAsync(request.ComboID.Value);
                    if (combo == null) return NotFound("Combo không tồn tại.");

                    var existingComboItem = await _context.CartItems.FirstOrDefaultAsync(ci =>
                        ci.CartId == cart.CartId && ci.ComboID == request.ComboID && ci.FoodID == null);

                    if (existingComboItem == null)
                    {
                        var newItem = new CartItem
                        {
                            CartItemId = Guid.NewGuid(),
                            CartId = cart.CartId,
                            ComboID = request.ComboID,
                            ProductName = combo.Name,
                            Price = combo.Price,
                            Quantity = request.Quantity
                        };
                        _context.CartItems.Add(newItem);
                        addedItems.Add(new { product = "combo", item = newItem });
                    }
                    else
                    {
                        existingComboItem.Quantity += request.Quantity;
                        existingComboItem.Price = combo.Price;
                        addedItems.Add(new { product = "combo", item = existingComboItem });
                    }
                }

                if (!addedItems.Any())
                    return BadRequest("Phải có ít nhất một trong FoodID hoặc ComboID.");

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "✔️ Đã thêm vào giỏ hàng.",
                    cartId = cart.CartId,
                    addedItems
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized("User ID không xác định.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
            }
        }


        // PUT: api/cart/update/{itemId}
        [HttpPut("update/{cartItemId}")]
        public async Task<IActionResult> UpdateCartItem(Guid cartItemId, [FromBody] UpdateCartItemDTO request)
        {
            try
            {
                if (request == null || request.Quantity <= 0)
                    return BadRequest("Số lượng sản phẩm phải lớn hơn 0.");

                var userId = GetUserId(); // Bạn phải đảm bảo có phương thức này để lấy UserId từ token

                // Tìm cartItem thuộc giỏ hàng của user hiện tại
                var cartItem = await _context.CartItems
                    .Include(ci => ci.Cart)
                    .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId && ci.Cart.UserId == userId);

                if (cartItem == null)
                    return NotFound("Sản phẩm không tồn tại trong giỏ hàng.");

                cartItem.Quantity = request.Quantity;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Cập nhật giỏ hàng thành công.",
                    cartItemId = cartItem.CartItemId,
                    newQuantity = cartItem.Quantity,
                    totalPrice = cartItem.Total
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized("Không tìm thấy UserId trong token.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật giỏ hàng", error = ex.Message });
            }
        }



        // DELETE: api/cart/remove/{cartItemId}
        [HttpDelete("remove/{cartItemId}")]
        public async Task<IActionResult> RemoveFromCart(Guid cartItemId)
        {
            var userId = GetUserId();
            var cartItem = await _context.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci =>
                    ci.Cart.UserId == userId &&
                    ci.CartItemId == cartItemId);

            if (cartItem == null)
                return NotFound("Sản phẩm không tồn tại trong giỏ hàng.");

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã xóa sản phẩm khỏi giỏ hàng." });
        }

        // DELETE: api/cart/clear
        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            var userId = GetUserId();
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
                return NotFound("Giỏ hàng trống.");

            _context.CartItems.RemoveRange(cart.CartItems);
            await _context.SaveChangesAsync();
            return Ok("Đã xóa toàn bộ giỏ hàng.");
        }

    }
}
