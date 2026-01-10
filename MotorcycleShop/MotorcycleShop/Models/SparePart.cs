using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MotorcycleShop.Models
{
    public class SparePart
    {
        [Key]
        public int SparePartId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        [StringLength(100)]
        public string SpareName { get; set; }

        [StringLength(100)]
        public string Category { get; set; }

        [StringLength(100)]
        public string CompatibleWith { get; set; }

        // Navigation property
        [ForeignKey("ProductId")]
        public Product Product { get; set; }
    }
}