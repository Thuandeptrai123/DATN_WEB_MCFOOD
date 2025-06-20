namespace DUANTOTNGHIEP.DTOS.Ingredient
{
    public class IngredientDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Unit { get; set; }
        public decimal QuantityInStock { get; set; }

        public Guid ProviderId { get; set; }
        public string ProviderName { get; set; }
    }

}
