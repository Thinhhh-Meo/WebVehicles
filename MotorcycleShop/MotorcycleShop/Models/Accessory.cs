using System.ComponentModel.DataAnnotations;

namespace MotorcycleShop.Models
{
    public class Accessory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        public int Quantity { get; set; }

        [StringLength(100)]
        public string Category { get; set; }

        [StringLength(200)]
        public string ImageUrl { get; set; }

        public bool IsActive { get; set; } = true;
    }
}