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

    [HttpPost("create")]
    public async Task<IActionResult> CreateInvoice([FromBody] CreateInvoiceDTO dto)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.CustomerId == dto.CustomerId);

        if (cart == null || cart.Items.Count == 0)
        {
            return BadRequest(new BaseResponse<string>
            {
                ErrorCode = 400,
                Message = "Giỏ hàng rỗng hoặc không tồn tại."
            });
        }

        // Tính tổng tiền giả lập (giả định mỗi món 50k)
        decimal total = 0;
        var invoiceItems = new List<InvoiceItem>();

        foreach (var item in cart.Items)
        {
            decimal unitPrice = 50000; // TODO: lấy từ bảng Foods hoặc Combos thực tế
            total += unitPrice * item.Quantity;

            invoiceItems.Add(new InvoiceItem
            {
                Id = Guid.NewGuid(),
                FoodId = item.FoodId,
                ComboId = item.ComboId,
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

        // Xóa giỏ hàng sau khi tạo hóa đơn
        _context.CartItems.RemoveRange(cart.Items);
        _context.Carts.Remove(cart);

        await _context.SaveChangesAsync();

        return Ok(new BaseResponse<Invoice> { Data = invoice });
    }

    [HttpGet("{customerId}")]
    public async Task<IActionResult> GetInvoicesByCustomer(Guid customerId)
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
                // thêm vô nếu muốn UwU 123
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

}
