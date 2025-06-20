namespace DUANTOTNGHIEP.DTOS.Recipe
{
    public class BulkUpdateRecipeDto
    {
        public Guid FoodId { get; set; }
        public List<IngredientUpdateDto> Ingredients { get; set; }
    }

    public class IngredientUpdateDto
    {
        public Guid IngredientId { get; set; }
        public decimal QuantityRequired { get; set; }
    }

}
