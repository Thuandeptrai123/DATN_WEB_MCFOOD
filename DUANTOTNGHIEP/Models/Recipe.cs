namespace DUANTOTNGHIEP.Models
{
    public class Recipe : BaseModel
    {
        public Guid Id { get; set; }

        public Guid FoodId { get; set; }
        public Food Food { get; set; }

        public Guid IngredientId { get; set; }
        public Ingredient Ingredient { get; set; }

        public decimal QuantityRequired { get; set; } // số lượng nguyên liệu cần cho 1 phần ăn
    }

}
