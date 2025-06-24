namespace DUANTOTNGHIEP.Models
{
    public class CartItem
    {
        public Guid CartItemId { get; set; }
        public Guid CartId { get; set; }

        public Guid? FoodId { get; set; }
        public Guid? ComboId { get; set; }
        public int Quantity { get; set; }

        public Cart Cart { get; set; }
    }
}
