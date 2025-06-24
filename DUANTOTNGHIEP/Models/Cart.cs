namespace DUANTOTNGHIEP.Models
{
    public class Cart : BaseModel
    {
        public Guid CartId { get; set; }
        public Guid CustomerId { get; set; }
        public List<CartItem> Items { get; set; } = new();
    }
}
