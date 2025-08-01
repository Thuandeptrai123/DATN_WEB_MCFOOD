﻿namespace DUANTOTNGHIEP.DTOS.Combo
{
    public class UpdateComboDto
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }  // Không dùng nhưng cần để parse Form
    }



    public class ComboFoodItemUpdateDto
    {
        public Guid FoodId { get; set; }
        public int Quantity { get; set; }
    }

}
