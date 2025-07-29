namespace DUANTOTNGHIEP.Models
{
    public class Food : BaseModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }

        public int? CookableQuantity { get; set; }

        public Guid FoodTypeId { get; set; }
        public FoodType FoodType { get; set; }

        public ICollection<Recipe> Recipes { get; set; }
    }

}
