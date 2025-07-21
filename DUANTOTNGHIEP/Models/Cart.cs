using System.ComponentModel.DataAnnotations;

namespace DUANTOTNGHIEP.Models
{
    public class Cart : BaseModel
    {
        [Key]
        public Guid CartId { get; set; } = Guid.NewGuid();

        [Required]
        public string UserId { get; set; }

        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }

}
