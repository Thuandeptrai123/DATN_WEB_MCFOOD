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
}
