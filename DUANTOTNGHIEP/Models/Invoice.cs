namespace DUANTOTNGHIEP.Models
{
    public class Invoice : BaseModel
    {
        public Guid Id { get; set; }
        public string CustomerId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } // ví dụ: "Pending", "Paid", "Cancelled"

        public List<InvoiceItem> Items { get; set; } = new();

        public ICollection<InvoiceHistory> Histories { get; set; }
    }
}
