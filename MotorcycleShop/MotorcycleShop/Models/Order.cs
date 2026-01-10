using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MotorcycleShop.Models
{
    public enum OrderStatus
    {
        Pending = 1,
        Confirmed = 2,
        Shipping = 3,
        Delivered = 4,
        Cancelled = 5
    }

    public enum PaymentMethod
    {
        COD = 1,
        BankTransfer = 2,
        CreditCard = 3
    }

    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [Required]
        public PaymentMethod PaymentMethod { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal TotalPrice { get; set; }

        [Range(0, double.MaxValue)]
        public decimal DiscountAmount { get; set; } = 0;

        [Required]
        [Range(0, double.MaxValue)]
        public decimal FinalPrice { get; set; }

        [StringLength(200)]
        public string ShippingAddress { get; set; }

        [StringLength(20)]
        public string PhoneNumber { get; set; }

        [StringLength(500)]
        public string? Note { get; set; }

        // Navigation properties
        public ApplicationUser User { get; set; }
        public ICollection<OrderDetail> OrderDetails { get; set; }
        public Discount? Discount { get; set; }
        public int? DiscountId { get; set; }
    }
}