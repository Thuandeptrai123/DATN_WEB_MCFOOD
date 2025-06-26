namespace DUANTOTNGHIEP.Models
{
    public class InvoiceItem
    {
        public Guid Id { get; set; }
        public Guid InvoiceId { get; set; }

        public Guid? FoodId { get; set; }
        public Guid? ComboId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        public Invoice Invoice { get; set; }
    }
}
