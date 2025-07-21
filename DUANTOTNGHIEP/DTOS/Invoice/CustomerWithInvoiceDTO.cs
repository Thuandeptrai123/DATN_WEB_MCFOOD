namespace DUANTOTNGHIEP.DTOS.Invoice
{
    public class CustomerWithInvoiceDTO
    {
        public string CustomerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public int InvoiceCount { get; set; }
        public DateTime LastInvoiceDate { get; set; }
    }

}
