namespace DUANTOTNGHIEP.DTOS.Recipe
{
    public class RecipeDto
    {
        public Guid FoodId { get; set; }
        public string FoodName { get; set; }

        public Guid IngredientId { get; set; }
        public string IngredientName { get; set; }

        public decimal QuantityRequired { get; set; }
        public string Unit { get; set; }
    }

}
