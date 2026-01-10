using System;
using System.ComponentModel.DataAnnotations;

namespace MotorcycleShop.Models
{
    public enum PaymentStatus
    {
        Pending = 1,
        Completed = 2,
        Failed = 3,
        Refunded = 4
    }

    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; }

        [StringLength(100)]
        public string PaymentMethod { get; set; } // COD, Bank Transfer, Credit Card

        [StringLength(200)]
        public string TransactionId { get; set; }

        [StringLength(500)]
        public string Note { get; set; }

        // Navigation property
        public Order Order { get; set; }
    }
}