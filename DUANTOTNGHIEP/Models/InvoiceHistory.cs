namespace DUANTOTNGHIEP.Models
{
    public class InvoiceHistory
    {
        public Guid Id { get; set; }
        public Guid InvoiceId { get; set; }
        public string Status { get; set; } // ví dụ: Pending, Paid, Cancelled
        public string Action { get; set; } // mô tả hành động: "Tạo đơn", "Thanh toán", "Hủy đơn"
        public string UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Invoice Invoice { get; set; } // liên kết với Invoice
    }

}
