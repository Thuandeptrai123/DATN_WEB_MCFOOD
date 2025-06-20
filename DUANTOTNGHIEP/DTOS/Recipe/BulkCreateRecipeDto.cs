namespace DUANTOTNGHIEP.DTOS.Recipe
{
    public class BulkCreateRecipeDto
    {
        public Guid FoodId { get; set; }
        public List<IngredientQuantityDto> Ingredients { get; set; }
    }

    public class IngredientQuantityDto
    {
        public Guid IngredientId { get; set; }
        public decimal QuantityRequired { get; set; }
    }

}
