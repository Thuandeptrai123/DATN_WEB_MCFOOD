using System.ComponentModel.DataAnnotations;

namespace DUANTOTNGHIEP.DTOS
{
    public class UpdateUser_DTO
    {
        [Required(ErrorMessage = "Tên người dùng không được để trống.")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Tên người dùng không được để trống.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Địa chỉ không được để trống.")]
        public string Address { get; set; }
        public IFormFile? ProfileImage { get; set; }
    }
}
