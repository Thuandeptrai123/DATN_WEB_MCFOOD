namespace DUANTOTNGHIEP.DTOS.StockTransaction
{
    public class StockTransactionDto
    {
        public Guid Id { get; set; }
        public Guid IngredientId { get; set; }
        public string IngredientName { get; set; }
        public string Type { get; set; } // Import / Export
        public decimal Quantity { get; set; }
        public DateTime Date { get; set; }
        public string? Note { get; set; }
    }
}
