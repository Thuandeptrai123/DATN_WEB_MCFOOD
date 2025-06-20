namespace DUANTOTNGHIEP.DTOS.Combo
{
    public class ComboDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }

        public List<ComboFoodItemDto> Items { get; set; }
    }

    public class ComboFoodItemDto
    {
        public Guid FoodId { get; set; }
        public string FoodName { get; set; }
        public int Quantity { get; set; }
    }

}
