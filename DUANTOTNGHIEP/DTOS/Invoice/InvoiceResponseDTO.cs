﻿namespace DUANTOTNGHIEP.DTOS.Invoice
{
    public class InvoiceResponseDTO
    {
        public Guid Id { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<InvoiceItemDTO> Items { get; set; }
    }

    public class InvoiceItemDTO
    {
        public Guid? FoodId { get; set; }
        public string? FoodName { get; set; }  // ✅ Thêm
        public Guid? ComboId { get; set; }
        public string? ComboName { get; set; } // ✅ Thêm
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

}
