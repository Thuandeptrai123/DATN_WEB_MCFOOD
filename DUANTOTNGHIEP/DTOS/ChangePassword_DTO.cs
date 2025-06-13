using System.ComponentModel.DataAnnotations;

namespace DUANTOTNGHIEP.DTOS
{
    public class ChangePassword_DTO
    {

        [Required(ErrorMessage = "Mật khẩu không được để trống.")]
        [StringLength(100, ErrorMessage = "Mật khẩu phải có ít nhất {2} ký tự.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }


        [Required(ErrorMessage = "Mật khẩu không được để trống.")]
        [StringLength(100, ErrorMessage = "Mật khẩu phải có ít nhất {2} ký tự.", MinimumLength = 6)]
        [DataType(DataType.Password)]

        public string NewPassword { get; set; }
    }
}
