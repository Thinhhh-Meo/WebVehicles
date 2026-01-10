using System.ComponentModel.DataAnnotations;

namespace MotorcycleShop.Models.ViewModels
{
    public class EditProfileViewModel
    {
        public string Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [Phone]
        [RegularExpression(@"^\d{1,10}$", ErrorMessage = "Phone number must be at most 10 digits")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Range(18, 100,ErrorMessage ="Age must be between 18 and 100")]
        [Required]
        public int? Age { get; set; }
        [Required]
        public string Address { get; set; }

        [Display(Name = "Avatar URL")]
        public string? AvatarUrl { get; set; }
    }
}
