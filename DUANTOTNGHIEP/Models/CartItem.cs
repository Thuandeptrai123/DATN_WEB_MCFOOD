using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DUANTOTNGHIEP.Models
{
    public class CartItem
    {
        [Key]
        public Guid CartItemId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid CartId { get; set; }

        [ForeignKey("CartId")]
        public virtual Cart Cart { get; set; }

        public Guid? FoodID { get; set; }
        public Guid? ComboID { get; set; }

        [Required]
        public string ProductName { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [NotMapped]
        public decimal Total => Quantity * Price;
    }

}
