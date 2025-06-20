namespace DUANTOTNGHIEP.Models
{
    public class Ingredient : BaseModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Unit { get; set; } // Ví dụ: gram, ml, quả, ...
        public decimal QuantityInStock { get; set; }

        public Guid ProviderId { get; set; }
        public Provider Provider { get; set; }

        public ICollection<Recipe> Recipes { get; set; }
        public ICollection<StockTransaction> StockTransactions { get; set; }
    }

}
