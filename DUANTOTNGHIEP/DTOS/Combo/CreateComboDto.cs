namespace DUANTOTNGHIEP.DTOS.Combo
{
    public class CreateComboDto
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public IFormFile? ImageFile { get; set; }

        public string ItemsJson { get; set; } // ⬅️ Nhận từ client
    }



    public class ComboFoodItemCreateDto
    {
        public Guid FoodId { get; set; }
        public int Quantity { get; set; }
    }

}
