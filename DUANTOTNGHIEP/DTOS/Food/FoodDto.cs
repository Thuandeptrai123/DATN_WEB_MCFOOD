namespace DUANTOTNGHIEP.DTOS.Food
{
    public class FoodDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }

        public int CookableQuantity { get; set; }
        public int CookedQuantity { get; set; }
        public bool IsActive { get; set; }
        public Guid FoodTypeId { get; set; }
        public string FoodTypeName { get; set; }
    }

}
