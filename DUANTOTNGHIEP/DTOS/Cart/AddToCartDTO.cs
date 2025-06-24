namespace DUANTOTNGHIEP.DTOS.Cart
{
    public class AddToCartDTO
    {
        public Guid CustomerId { get; set; }
        public Guid? FoodId { get; set; }
        public Guid? ComboId { get; set; }
        public int Quantity { get; set; }
    }
}
