﻿using System.ComponentModel.DataAnnotations;

namespace DUANTOTNGHIEP.DTOS
{
    public class UserRegister_DTO
    {
        [Required(ErrorMessage = "Tên người dùng không được để trống.")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Tên người dùng không được để trống.")]
        public string LastName { get; set; }
        public string? UserName { get; set; }
        [Required(ErrorMessage = "Số điện thoại không được để trống.")]
        [RegularExpression(@"^[0-9]{9,11}$", ErrorMessage = "Số điện thoại không hợp lệ.")]
        public string PhoneNumbers { get; set; } // ✅ Đổi từ int → string
        [Required(ErrorMessage = "Email không được để trống.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống.")]
        [StringLength(100, ErrorMessage = "Mật khẩu phải có ít nhất {2} ký tự.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Địa chỉ không được để trống.")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu.")]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        [DataType(DataType.Password)]

        public string ConfirmPassword { get; set; }

        public IFormFile? ProfileImage { get; set; }
    }
}
