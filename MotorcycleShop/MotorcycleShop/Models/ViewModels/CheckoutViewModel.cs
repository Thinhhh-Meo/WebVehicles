using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MotorcycleShop.Models.ViewModels
{
    public class CheckoutViewModel
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        [Required]
        [StringLength(200)]
        public string Address { get; set; }

        [StringLength(500, ErrorMessage = "Note cannot exceed 500 characters")]
        public string? Note { get; set; } 
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.COD;

        [StringLength(50, ErrorMessage = "Discount code cannot exceed 50 characters")]

        [BindProperty]
        public string? DiscountCode { get; set; }

        public List<CartItem> CartItems { get; set; } = new List<CartItem>();
        public decimal TotalPrice { get; set; }

        [BindProperty]
        public decimal DiscountAmount { get; set; }
        [BindProperty]
        public decimal FinalPrice { get; set; }
    }
}