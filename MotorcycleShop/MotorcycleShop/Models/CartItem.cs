using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MotorcycleShop.Models
{
    public class CartItem
    {
        [Key]
        public int CartItemId { get; set; }
        [Required]
        public int CartId { get; set; }

        [Required]

        public int ProductId { get; set; }

        [Required]
        [Range(1, 100)]
        public int Quantity { get; set; }

        public DateTime AddedDate { get; set; } = DateTime.Now;

        // Navigation properties
        

        [ForeignKey("CartId")]
        public Cart Cart { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; }
    }
}