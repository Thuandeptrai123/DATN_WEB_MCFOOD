namespace DUANTOTNGHIEP.DTOS
{
    public class FoodType_DTO
    {
        public Guid FoodTypeId { get; set; }
        public string FoodTypeName { get; set; }

        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }
    }
}
