namespace DUANTOTNGHIEP.DTOS.Ingredient
{
    public class UpdateIngredientDto
    {
        public string Name { get; set; }
        public string Unit { get; set; }
        public decimal QuantityInStock { get; set; }
        public Guid ProviderId { get; set; }
    }

}
