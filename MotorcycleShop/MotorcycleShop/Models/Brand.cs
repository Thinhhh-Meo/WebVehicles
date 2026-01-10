using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MotorcycleShop.Models
{
    public class Brand
    {
        [Key]
        public int BrandId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }  // Tên thương hiệu

        [StringLength(500)]
        public string Description { get; set; }

        [StringLength(200)]
        public string LogoUrl { get; set; }

        [StringLength(100)]
        public string Country { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime EstablishedYear { get; set; }

        // Navigation properties
        public ICollection<Product> Products { get; set; }
    }
}