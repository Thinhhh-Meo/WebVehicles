using System.ComponentModel.DataAnnotations;

namespace MotorcycleShop.Models
{
    public class Promotion
    {
        [Key]
        public int PromotionId { get; set; }

        [Required]
        [StringLength(200)]
        public string Condition { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        [StringLength(500)]
        public string ImagePath { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        public int? DiscountId { get; set; }
        [Range(0, 100)]
        public int DisplayOrder { get; set; } = 0;

        // Navigation property
        public Discount? Discount { get; set; }
    }
}