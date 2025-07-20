namespace DUANTOTNGHIEP.Models
{
    public class ComboDetail : BaseModel
    {
        public Guid Id { get; set; }

        public Guid ComboId { get; set; }
        public Combo Combo { get; set; }

        public Guid FoodId { get; set; }
        public Food Food { get; set; }

        public int Quantity { get; set; }
    }
}
