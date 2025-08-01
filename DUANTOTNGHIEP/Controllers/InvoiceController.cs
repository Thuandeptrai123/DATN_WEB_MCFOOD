using DUANTOTNGHIEP.Data;
using DUANTOTNGHIEP.DTOS.BaseResponses;
using DUANTOTNGHIEP.DTOS.Invoice;
using DUANTOTNGHIEP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class InvoiceController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public InvoiceController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    private string GetUserId()
    {
        var userIdClaim = User.Claims.FirstOrDefault(c =>
            c.Type == ClaimTypes.Sid || // Đây là dạng đã được dùng trong login
            c.Type.EndsWith("sid"));    // dự phòng cho kiểu định danh đầy đủ

        if (userIdClaim == null)
            throw new UnauthorizedAccessException("Không tìm thấy userId trong token.");

        return userIdClaim.Value;
    }

    [HttpGet("customers-with-invoices")]
    public async Task<IActionResult> GetCustomersWithInvoices()
    {
        var result = await _context.Invoices
            .GroupBy(i => i.CustomerId)
            .Select(g => new
            {
                CustomerId = g.Key,
                InvoiceCount = g.Count(),
                LastInvoiceDate = g.Max(i => i.CreatedDate)
            })
            .Join(_context.Users,
                invoiceGroup => invoiceGroup.CustomerId,
                user => user.Id,
                (invoiceGroup, user) => new CustomerWithInvoiceDTO
                {
                    CustomerId = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    InvoiceCount = invoiceGroup.InvoiceCount,
                    LastInvoiceDate = invoiceGroup.LastInvoiceDate
                })
            .OrderByDescending(c => c.LastInvoiceDate)
            .ToListAsync();

        return Ok(new BaseResponse<List<CustomerWithInvoiceDTO>> { Data = result });
    }


    //[HttpPost("create")]
    //public async Task<IActionResult> CreateInvoice([FromBody] CreateInvoiceDTO dto)
    //{
    //    var cart = await _context.Carts
    //        .Include(c => c.CartItems) // SỬA Ở ĐÂY
    //        .FirstOrDefaultAsync(c => c.UserId == dto.CustomerId); // SỬA CustomerId thành UserId nếu theo model

    //    if (cart == null || cart.CartItems.Count == 0) // SỬA Ở ĐÂY
    //    {
    //        return BadRequest(new BaseResponse<string>
    //        {
    //            ErrorCode = 400,
    //            Message = "Giỏ hàng rỗng hoặc không tồn tại."
    //        });
    //    }

    //    decimal total = 0;
    //    var invoiceItems = new List<InvoiceItem>();

    //    foreach (var item in cart.CartItems) // SỬA Ở ĐÂY
    //    {
    //        decimal unitPrice = 50000;
    //        total += unitPrice * item.Quantity;

    //        invoiceItems.Add(new InvoiceItem
    //        {
    //            Id = Guid.NewGuid(),
    //            FoodId = item.FoodID, // SỬA FoodId thành FoodID theo model bạn gửi
    //            ComboId = item.ComboID,
    //            Quantity = item.Quantity,
    //            UnitPrice = unitPrice
    //        });
    //    }

    //    var invoice = new Invoice
    //    {
    //        Id = Guid.NewGuid(),
    //        CustomerId = dto.CustomerId,
    //        CreatedDate = DateTime.Now,
    //        CreatedBy = "system",
    //        UpdatedDate = DateTime.Now,
    //        UpdatedBy = "system",
    //        Status = "Pending",
    //        TotalAmount = total,
    //        Items = invoiceItems
    //    };

    //    _context.Invoices.Add(invoice);

    //    _context.CartItems.RemoveRange(cart.CartItems); // SỬA Ở ĐÂY
    //    _context.Carts.Remove(cart);

    //    await _context.SaveChangesAsync();

    //    return Ok(new BaseResponse<Invoice> { Data = invoice });
    //}

    //[HttpPost("create")]
    //public async Task<IActionResult> CreateInvoice([FromBody] CreateInvoiceDTO dto)
    //{
    //    var cart = await _context.Carts
    //        .Include(c => c.CartItems)
    //        .FirstOrDefaultAsync(c => c.UserId == dto.CustomerId);

    //    if (cart == null || cart.CartItems.Count == 0)
    //    {
    //        return BadRequest(new BaseResponse<string>
    //        {
    //            ErrorCode = 400,
    //            Message = "❌ Giỏ hàng rỗng hoặc không tồn tại."
    //        });
    //    }

    //    decimal total = 0;
    //    var invoiceItems = new List<InvoiceItem>();

    //    foreach (var item in cart.CartItems)
    //    {
    //        decimal unitPrice = item.Price; // ✅ lấy đúng giá từ CartItem
    //        total += unitPrice * item.Quantity;

    //        invoiceItems.Add(new InvoiceItem
    //        {
    //            Id = Guid.NewGuid(),
    //            FoodId = item.FoodID,
    //            ComboId = item.ComboID,
    //            Quantity = item.Quantity,
    //            UnitPrice = unitPrice
    //        });
    //    }

    //    var invoice = new Invoice
    //    {
    //        Id = Guid.NewGuid(),
    //        CustomerId = dto.CustomerId,
    //        CreatedDate = DateTime.Now,
    //        CreatedBy = "system",
    //        UpdatedDate = DateTime.Now,
    //        UpdatedBy = "system",
    //        Status = "Pending",
    //        TotalAmount = total,
    //        Items = invoiceItems
    //    };

    //    _context.Invoices.Add(invoice);

    //    // ✅ Xoá cart items & cart sau khi tạo hoá đơn
    //    _context.CartItems.RemoveRange(cart.CartItems);
    //    _context.Carts.Remove(cart);

    //    await _context.SaveChangesAsync();

    //    return Ok(new BaseResponse<Invoice>
    //    {
    //        Message = "✅ Tạo hóa đơn thành công.",
    //        Data = invoice
    //    });
    //}

    [HttpPost("create")]
    public async Task<IActionResult> CreateInvoice([FromBody] CreateInvoiceDTO dto)
    {
        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == dto.CustomerId);

        if (cart == null || cart.CartItems.Count == 0)
        {
            return BadRequest(new BaseResponse<string>
            {
                ErrorCode = 400,
                Message = "❌ Giỏ hàng rỗng hoặc không tồn tại."
            });
        }

        decimal total = 0;
        var invoiceItems = new List<InvoiceItem>();

        // Check và trừ số lượng CookedQuantity
        foreach (var item in cart.CartItems)
        {
            if (item.FoodID.HasValue)
            {
                var food = await _context.Foods.FindAsync(item.FoodID.Value);
                if (food == null)
                    return BadRequest(new { message = "Món ăn không tồn tại!" });

                if (food.CookedQuantity < item.Quantity)
                {
                    return BadRequest(new { message = $"Không đủ món đã nấu cho: {food.Name}" });
                }

                food.CookedQuantity -= item.Quantity;
            }

            total += item.Price * item.Quantity;

            invoiceItems.Add(new InvoiceItem
            {
                Id = Guid.NewGuid(),
                FoodId = item.FoodID,
                ComboId = item.ComboID,
                Quantity = item.Quantity,
                UnitPrice = item.Price
            });
        }

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            CustomerId = dto.CustomerId,
            CreatedDate = DateTime.Now,
            CreatedBy = "system",
            UpdatedDate = DateTime.Now,
            UpdatedBy = "system",
            Status = "Pending",
            TotalAmount = total,
            Items = invoiceItems
        };

        _context.Invoices.Add(invoice);

        // Xóa giỏ hàng
        _context.CartItems.RemoveRange(cart.CartItems);
        cart.CartItems.Clear();
        _context.Carts.Update(cart);

        await _context.SaveChangesAsync();

        return Ok(new BaseResponse<Invoice>
        {
            Message = "✅ Tạo hóa đơn thành công.",
            Data = invoice
        });
    }





    [HttpGet("{customerId}")]
    public async Task<IActionResult> GetInvoicesByCustomer(string customerId)
    {
        var invoices = await _context.Invoices
            .Include(i => i.Items)
            .Where(i => i.CustomerId == customerId)
            .OrderByDescending(i => i.CreatedDate)
            .ToListAsync();

        return Ok(new BaseResponse<List<Invoice>> { Data = invoices });
    }

    //[HttpGet("detail/{invoiceId}")]
    //public async Task<IActionResult> GetInvoiceDetail(Guid invoiceId)
    //{
    //    var invoice = await _context.Invoices
    //        .Include(i => i.Items)
    //        .FirstOrDefaultAsync(i => i.Id == invoiceId);

    //    if (invoice == null)
    //    {
    //        return NotFound(new BaseResponse<string>
    //        {
    //            ErrorCode = 404,
    //            Message = "Không tìm thấy hóa đơn."
    //        });
    //    }

    //    return Ok(new BaseResponse<Invoice> { Data = invoice });
    //}

    [HttpGet("detail/{invoiceId}")]
    public async Task<IActionResult> GetInvoiceDetail(Guid invoiceId)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == invoiceId);

        if (invoice == null)
        {
            return NotFound(new BaseResponse<string>
            {
                ErrorCode = 404,
                Message = "Không tìm thấy hóa đơn."
            });
        }

        // Lấy danh sách Food và Combo từ DB (nếu cần thiết)
        var foodDict = await _context.Foods.ToDictionaryAsync(f => f.Id, f => f.Name);
        var comboDict = await _context.Combos.ToDictionaryAsync(c => c.Id, c => c.Name);

        var invoiceDto = new InvoiceResponseDTO
        {
            Id = invoice.Id,
            CreatedDate = invoice.CreatedDate,
            Status = invoice.Status,
            TotalAmount = invoice.TotalAmount,
            Items = invoice.Items.Select(item => new InvoiceItemDTO
            {
                FoodId = item.FoodId,
                FoodName = item.FoodId != null && foodDict.ContainsKey(item.FoodId.Value)
                            ? foodDict[item.FoodId.Value]
                            : null,
                ComboId = item.ComboId,
                ComboName = item.ComboId != null && comboDict.ContainsKey(item.ComboId.Value)
                            ? comboDict[item.ComboId.Value]
                            : null,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            }).ToList()
        };

        return Ok(new BaseResponse<InvoiceResponseDTO>
        {
            Data = invoiceDto
        });
    }





    [HttpPut("update-status/{invoiceId}")]
    public async Task<IActionResult> UpdateInvoiceStatus(
    Guid invoiceId,
    [FromQuery] string newStatus,
    [FromQuery] string action = "Cập nhật trạng thái",
    [FromQuery] string updatedBy = "admin")
    {
        var invoice = await _context.Invoices
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == invoiceId);

        if (invoice == null)
        {
            return NotFound(new BaseResponse<string>
            {
                ErrorCode = 404,
                Message = "Không tìm thấy hóa đơn để cập nhật."
            });
        }

        var oldStatus = invoice.Status;

        invoice.Status = newStatus;
        invoice.UpdatedDate = DateTime.Now;
        invoice.UpdatedBy = updatedBy;

        _context.Invoices.Update(invoice);

        // Ghi lại lịch sử thay đổi
        _context.InvoiceHistories.Add(new InvoiceHistory
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoice.Id,
            Status = newStatus,
            Action = action,
            UpdatedBy = updatedBy,
            UpdatedAt = DateTime.Now
        });

        await _context.SaveChangesAsync();

        return Ok(new BaseResponse<string>
        {
            Message = $"✅ Hóa đơn đã cập nhật từ '{oldStatus}' ➜ '{newStatus}' với hành động: {action}"
        });
    }



    //[HttpPost("process-payment/{invoiceId}")]
    //public async Task<IActionResult> ProcessPayment(Guid invoiceId, [FromQuery] string paymentMethod)
    //{
    //    var invoice = await _context.Invoices
    //        .Include(i => i.Items)
    //        .FirstOrDefaultAsync(i => i.Id == invoiceId);

    //    if (invoice == null)
    //    {
    //        return NotFound(new BaseResponse<string>
    //        {
    //            ErrorCode = 404,
    //            Message = "❌ Không tìm thấy hóa đơn."
    //        });
    //    }

    //    var invoiceSummary = new InvoiceResponseDTO
    //    {
    //        Id = invoice.Id,
    //        CreatedDate = invoice.CreatedDate,
    //        TotalAmount = invoice.TotalAmount,
    //        Status = invoice.Status,
    //        Items = invoice.Items.Select(i => new InvoiceItemDTO
    //        {
    //            FoodId = i.FoodId,
    //            ComboId = i.ComboId,
    //            Quantity = i.Quantity,
    //            UnitPrice = i.UnitPrice
    //        }).ToList()
    //    };

    //    string paymentRedirectUrl = "";
    //    switch (paymentMethod)
    //    {
    //        case "MoMo":
    //            paymentRedirectUrl = $"https://momo.vn/checkout?invoiceId={invoice.Id}";
    //            break;
    //            // thêm vô nếu muốn UwU
    //        //case "VNPay":
    //        //    paymentRedirectUrl = $"https://vnpay.vn/checkout?invoiceId={invoice.Id}";
    //        //    break;
    //        case "COD":
    //            invoice.Status = "Paid";
    //            invoice.UpdatedDate = DateTime.Now;
    //            invoice.UpdatedBy = "system";
    //            _context.Invoices.Update(invoice);
    //            await _context.SaveChangesAsync();

    //            return Ok(new BaseResponse<InvoiceResponseDTO>
    //            {
    //                Message = "Thanh toán COD thành công.",
    //                Data = invoiceSummary
    //            });

    //        default:
    //            return BadRequest(new BaseResponse<string>
    //            {
    //                ErrorCode = 400,
    //                Message = "Phương thức thanh toán không hợp lệ. Vui lòng chọn: MoMo hoặc COD."
    //            });
    //    }

    //    return Ok(new BaseResponse<object>
    //    {
    //        Message = "Chuyển hướng đến cổng thanh toán.",
    //        Data = new
    //        {
    //            Invoice = invoiceSummary,
    //            PaymentRedirectUrl = paymentRedirectUrl
    //        }
    //    });
    //}


    [HttpPost("process-payment/{invoiceId}")]
    public async Task<IActionResult> ProcessPayment(Guid invoiceId, [FromQuery] string paymentMethod)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == invoiceId);

        if (invoice == null)
        {
            return NotFound(new BaseResponse<string>
            {
                ErrorCode = 404,
                Message = "❌ Không tìm thấy hóa đơn."
            });
        }

        if (paymentMethod != "COD")
        {
            return BadRequest(new BaseResponse<string>
            {
                ErrorCode = 400,
                Message = "❌ Phương thức thanh toán không hợp lệ. Chỉ hỗ trợ COD."
            });
        }

        // Xử lý thanh toán COD
        invoice.Status = "Paid";
        invoice.UpdatedDate = DateTime.Now;
        invoice.UpdatedBy = "system";
        _context.Invoices.Update(invoice);
        await _context.SaveChangesAsync();

        var invoiceSummary = new InvoiceResponseDTO
        {
            Id = invoice.Id,
            CreatedDate = invoice.CreatedDate,
            TotalAmount = invoice.TotalAmount,
            Status = invoice.Status,
            Items = invoice.Items.Select(i => new InvoiceItemDTO
            {
                FoodId = i.FoodId,
                ComboId = i.ComboId,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList()
        };

        return Ok(new BaseResponse<InvoiceResponseDTO>
        {
            Message = "✅ Thanh toán COD thành công.",
            Data = invoiceSummary
        });
    }


    [HttpGet("my-cart")]
    [Authorize] // Đảm bảo người dùng đã đăng nhập
    public async Task<IActionResult> GetMyCart()
    {
        // Lấy userId từ JWT claims
        //var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userId = GetUserId(); // Lấy userId từ token

        if (userId == "" || userId == null)
        {
            return Unauthorized(new { message = "Người dùng chưa đăng nhập." });
        }

        //if (string.IsNullOrEmpty(userId))
        //    return Unauthorized(new { Message = "Không thể xác định người dùng." });

        // Lấy thông tin người dùng
        var user = await _userManager.Users
            .Where(u => u.Id == userId)
            .Select(u => new
            {
                u.Id,
                u.UserName,
                u.Email,
                u.FirstName,
                u.LastName,
                u.Address,
                u.PhoneNumbers,
                u.ProfileImage,
                u.IsEmployee,
                u.IsActive
            })
            .FirstOrDefaultAsync();

        if (user == null)
            return NotFound(new { Message = "Không tìm thấy người dùng." });

        // Lấy giỏ hàng + sản phẩm trong giỏ
        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
            return Ok(new
            {
                User = user,
                Cart = new { Message = "Giỏ hàng trống." }
            });

        return Ok(new
        {
            User = user,
            Cart = new
            {
                cart.CartId,
                cart.CreatedDate,
                Items = cart.CartItems.Select(item => new
                {
                    item.CartItemId,
                    item.ProductName,
                    item.Quantity,
                    item.Price,
                    item.Total,
                    item.FoodID,
                    item.ComboID
                }),
                TotalQuantity = cart.CartItems.Sum(item => item.Quantity),
                TotalAmount = cart.CartItems.Sum(item => item.Quantity * item.Price)

            }
        });
    }

    [HttpGet("statistics/invoices-by-month")]
    public async Task<IActionResult> GetMonthlyInvoiceCount()
    {
        var stats = await _context.Invoices
            .GroupBy(i => new { i.CreatedDate.Year, i.CreatedDate.Month })
            .Select(g => new
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                TotalInvoices = g.Count()
            })
            .OrderBy(g => g.Year).ThenBy(g => g.Month)
            .ToListAsync();

        return Ok(new
        {
            ErrorCode = 0,
            Message = (string)null,
            Data = stats
        });
    }

    [HttpGet("statistics/customers-by-month")]
    public async Task<IActionResult> GetMonthlyCustomerStats()
    {
        var stats = await _context.Invoices
            .GroupBy(i => new { i.CreatedDate.Year, i.CreatedDate.Month })
            .Select(g => new
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                TotalCustomers = g.Select(i => i.CustomerId).Distinct().Count()
            })
            .OrderBy(g => g.Year).ThenBy(g => g.Month)
            .ToListAsync();

        return Ok(new
        {
            ErrorCode = 0,
            Message = (string)null,
            Data = stats
        });
    }

    [HttpGet("statistics/revenue-by-month")]
    public async Task<IActionResult> GetMonthlyRevenueStats()
    {
        var revenueStats = await _context.InvoiceHistories
            .Where(h => h.Status == "Paid")
            .Join(_context.Invoices,
                  history => history.InvoiceId,
                  invoice => invoice.Id,
                  (history, invoice) => new { history, invoice })
            .GroupBy(x => new { x.history.UpdatedAt.Year, x.history.UpdatedAt.Month })
            .Select(g => new
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                TotalRevenue = g.Sum(x => x.invoice.TotalAmount)
            })
            .OrderBy(g => g.Year).ThenBy(g => g.Month)
            .ToListAsync();

        return Ok(new { ErrorCode = 0, Message = (string)null, Data = revenueStats });
    }
}
