using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MotorcycleShop.Models
{
    public enum DiscountType
    {
        Percentage = 1,
        FixedAmount = 2
    }

    public enum DiscountValueType
    {
        AllProducts = 1,
        SpecificCategory = 2,
        SpecificProduct = 3,
        MinOrderAmount = 4
    }

    public class Discount
    {
        [Key]
        public int DiscountId { get; set; }

        [Required]
        [StringLength(100)]
        public string DiscountName { get; set; }

        [Required]
        [StringLength(50)]
        public string Code { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public DateTime DateStart { get; set; }

        [Required]
        public DateTime DateEnd { get; set; }

        [Required]
        public DiscountType DiscountType { get; set; }

        [Required]
        [Range(0, float.MaxValue)]
        public float DiscountValue { get; set; }

        public DiscountValueType DiscountValueType { get; set; } = DiscountValueType.AllProducts;

        [Range(0, double.MaxValue)]
        public decimal? MinOrderAmount { get; set; }

        public bool IsActive { get; set; } = true;

        public int? UsageLimit { get; set; }
        public int UsedCount { get; set; } = 0;

        // Navigation properties
        public ICollection<Order> Orders { get; set; }
        public ICollection<Coupon> Coupons { get; set; }
        public ICollection<Promotion> Promotions { get; set; }

        // **THÊM DÒNG NÀY**
        public ICollection<UserDiscount> UserDiscounts { get; set; }
    }
}