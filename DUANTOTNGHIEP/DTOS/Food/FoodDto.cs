namespace DUANTOTNGHIEP.DTOS.Food
{
    public class FoodDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }

        public Guid FoodTypeId { get; set; }
        public string FoodTypeName { get; set; }
    }

}
