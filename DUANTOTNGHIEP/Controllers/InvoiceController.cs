using DUANTOTNGHIEP.Data;
using DUANTOTNGHIEP.DTOS.BaseResponses;
using DUANTOTNGHIEP.DTOS.Invoice;
using DUANTOTNGHIEP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class InvoiceController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public InvoiceController(ApplicationDbContext context)
    {
        _context = context;
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


    [HttpPost("create")]
    public async Task<IActionResult> CreateInvoice([FromBody] CreateInvoiceDTO dto)
    {
        var cart = await _context.Carts
            .Include(c => c.CartItems) // SỬA Ở ĐÂY
            .FirstOrDefaultAsync(c => c.UserId == dto.CustomerId); // SỬA CustomerId thành UserId nếu theo model

        if (cart == null || cart.CartItems.Count == 0) // SỬA Ở ĐÂY
        {
            return BadRequest(new BaseResponse<string>
            {
                ErrorCode = 400,
                Message = "Giỏ hàng rỗng hoặc không tồn tại."
            });
        }

        decimal total = 0;
        var invoiceItems = new List<InvoiceItem>();

        foreach (var item in cart.CartItems) // SỬA Ở ĐÂY
        {
            decimal unitPrice = 50000;
            total += unitPrice * item.Quantity;

            invoiceItems.Add(new InvoiceItem
            {
                Id = Guid.NewGuid(),
                FoodId = item.FoodID, // SỬA FoodId thành FoodID theo model bạn gửi
                ComboId = item.ComboID,
                Quantity = item.Quantity,
                UnitPrice = unitPrice
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

        _context.CartItems.RemoveRange(cart.CartItems); // SỬA Ở ĐÂY
        _context.Carts.Remove(cart);

        await _context.SaveChangesAsync();

        return Ok(new BaseResponse<Invoice> { Data = invoice });
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

        return Ok(new BaseResponse<Invoice> { Data = invoice });
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

        string paymentRedirectUrl = "";
        switch (paymentMethod)
        {
            case "MoMo":
                paymentRedirectUrl = $"https://momo.vn/checkout?invoiceId={invoice.Id}";
                break;
                // thêm vô nếu muốn UwU
            //case "VNPay":
            //    paymentRedirectUrl = $"https://vnpay.vn/checkout?invoiceId={invoice.Id}";
            //    break;
            case "COD":
                invoice.Status = "Paid";
                invoice.UpdatedDate = DateTime.Now;
                invoice.UpdatedBy = "system";
                _context.Invoices.Update(invoice);
                await _context.SaveChangesAsync();

                return Ok(new BaseResponse<InvoiceResponseDTO>
                {
                    Message = "Thanh toán COD thành công.",
                    Data = invoiceSummary
                });

            default:
                return BadRequest(new BaseResponse<string>
                {
                    ErrorCode = 400,
                    Message = "Phương thức thanh toán không hợp lệ. Vui lòng chọn: MoMo hoặc COD."
                });
        }

        return Ok(new BaseResponse<object>
        {
            Message = "Chuyển hướng đến cổng thanh toán.",
            Data = new
            {
                Invoice = invoiceSummary,
                PaymentRedirectUrl = paymentRedirectUrl
            }
        });
    }

    [HttpGet("statistics/revenue-by-month")]
    public async Task<IActionResult> GetMonthlyRevenue()
    {
        var revenueStats = await _context.InvoiceHistories
            .Where(h => h.Status == "Paid")
            .GroupBy(h => new { h.UpdatedAt.Year, h.UpdatedAt.Month })
            .Select(g => new
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                TotalRevenue = g.Sum(h => h.Invoice.TotalAmount)
            })
            .OrderBy(g => g.Year).ThenBy(g => g.Month)
            .ToListAsync();

        return Ok(new BaseResponse<object> { Data = revenueStats });
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


}
