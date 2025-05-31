using System.ComponentModel.DataAnnotations;

namespace DUANTOTNGHIEP.DTOS
{
    public class CreateFoodType_DTO
    {
        [Required]
        [StringLength(100)]
        public string FoodTypeName { get; set; }

        [Required]
        [StringLength(255)]
        public string Description { get; set; }
    }
}
