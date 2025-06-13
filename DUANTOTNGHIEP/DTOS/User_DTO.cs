using System.ComponentModel.DataAnnotations;

namespace DUANTOTNGHIEP.DTOS
{
    public class User_DTO
    {
        public string  Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string? ProfileImage { get; set; }
    }
}
