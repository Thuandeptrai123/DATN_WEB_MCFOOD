using System.ComponentModel.DataAnnotations;

namespace DUANTOTNGHIEP.DTOS
{
    public class ProviderUpdateDTO
    {
        [Required]
        public string Name { get; set; }

        public string Address { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [Phone]
        public string Phone { get; set; }

        public string Description { get; set; }
    }
}
