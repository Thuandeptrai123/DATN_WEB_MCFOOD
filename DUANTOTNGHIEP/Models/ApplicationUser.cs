using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace DUANTOTNGHIEP.Models
{
    public class ApplicationUser: IdentityUser
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Address { get; set; }
        public string PhoneNumbers { get; set; }

        public string? ProfileImage { get; set; }

        public bool IsEmployee { get; set; }   // true nếu là nhân viên

        public bool IsActive { get; set; }
    }
}
