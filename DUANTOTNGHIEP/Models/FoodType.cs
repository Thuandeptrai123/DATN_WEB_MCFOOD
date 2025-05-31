namespace DUANTOTNGHIEP.Models
{
    public class FoodType : BaseModel
    {
        public Guid FoodTypeId { get; set; }
        public string FoodTypeName { get; set; }

        public string Description { get; set; }
    }
}
