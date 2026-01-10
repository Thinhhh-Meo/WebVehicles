#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MotorcycleShop.Models;

namespace MotorcycleShop.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ForgotPasswordModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var user = await _userManager.FindByEmailAsync(Input.Email);

            // Nếu user không tồn tại, vẫn redirect sang SecurityQuestion để không lộ thông tin
            if (user == null)
                return RedirectToPage("./SecurityQuestion");

            // Lưu UserId tạm thời
            TempData["UserId"] = user.Id;

            // Chuyển sang trang SecurityQuestion
            return RedirectToPage("./SecurityQuestion",new { userId = user.Id });
        }
    }
}
