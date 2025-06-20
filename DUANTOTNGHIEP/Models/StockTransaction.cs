namespace DUANTOTNGHIEP.Models
{
    public class StockTransaction : BaseModel
    {
        public Guid Id { get; set; }

        public Guid IngredientId { get; set; }
        public Ingredient Ingredient { get; set; }

        public string Type { get; set; } // "Import" hoặc "Export"
        public decimal Quantity { get; set; }
        public string? Note { get; set; }
        public DateTime Date { get; set; }
    }

}
