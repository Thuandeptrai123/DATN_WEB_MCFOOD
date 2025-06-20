namespace DUANTOTNGHIEP.DTOS.Recipe
{
    public class FoodRecipeDto
    {
        public Guid FoodId { get; set; }
        public string FoodName { get; set; }
        public List<IngredientInRecipeDto> Ingredients { get; set; }
    }

    public class IngredientInRecipeDto
    {
        public Guid IngredientId { get; set; }
        public string IngredientName { get; set; }
        public decimal QuantityRequired { get; set; }
        public string Unit { get; set; }
    }

}
