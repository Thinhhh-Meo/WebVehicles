using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MotorcycleShop.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Range(18, 100, ErrorMessage = "Age must be between 18 and 100")]
        public int? Age { get; set; }

        [Required(ErrorMessage = "Address is required for delivery")]
        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
        public string Address { get; set; }

        public string? AvatarUrl { get; set; }

        public string? Note { get; set; }

        public string? SecurityQuestion { get; set; }
        public string? SecurityAnswerHash { get; set; }

        // Navigation properties
        public ICollection<Order> Orders { get; set; }
        public ICollection<Feedback> Feedbacks { get; set; }
        public ICollection<Cart> Carts { get; set; }

        public virtual ICollection<UserDiscount> UserDiscounts { get; set; }
    }
}