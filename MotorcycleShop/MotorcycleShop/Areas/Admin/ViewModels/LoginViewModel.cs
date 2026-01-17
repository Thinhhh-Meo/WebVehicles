// Areas/Admin/ViewModels/LoginViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace MotorcycleShop.Areas.Admin.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Email is incorrect")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Rememeber Sign-in")]
        public bool RememberMe { get; set; }

        public string ReturnUrl { get; set; }
    }
}