using System.ComponentModel.DataAnnotations;

namespace MotorcycleShop.Models
{
    public class Coupon
    {
        [Key]
        public int CouponId { get; set; }

        [Required]
        [StringLength(50)]
        public string CouponCode { get; set; }

        public int DiscountId { get; set; }

        public bool IsUsed { get; set; } = false;
        public DateTime? UsedDate { get; set; }

        public string? UserId { get; set; }

        // Navigation properties
        public Discount Discount { get; set; }
        public ApplicationUser? User { get; set; }
    }
}