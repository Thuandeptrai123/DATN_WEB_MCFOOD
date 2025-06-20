namespace DUANTOTNGHIEP.DTOS.StockTransaction
{
    public class CreateStockTransactionDto
    {
        public Guid IngredientId { get; set; }
        public string Type { get; set; } // Import / Export
        public decimal Quantity { get; set; }
        public string? Note { get; set; }
    }

}
