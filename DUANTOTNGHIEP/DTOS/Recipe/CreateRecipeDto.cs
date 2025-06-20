namespace DUANTOTNGHIEP.DTOS.Recipe
{
    public class CreateRecipeDto
    {
        public Guid FoodId { get; set; }
        public Guid IngredientId { get; set; }
        public decimal QuantityRequired { get; set; }
    }

}
