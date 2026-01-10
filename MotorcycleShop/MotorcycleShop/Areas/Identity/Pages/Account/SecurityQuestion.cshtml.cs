using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MotorcycleShop.Models;

namespace MotorcycleShop.Areas.Identity.Pages.Account
{
    public class SecurityQuestionModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public SecurityQuestionModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty(SupportsGet = true)]
        public string UserId { get; set; }

        [BindProperty]
        public string Answer { get; set; }

        public string Question { get; set; }
        public string? Error { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (TempData["UserId"] == null)
                return RedirectToPage("./ForgotPassword");

            var userId = TempData["UserId"].ToString();
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null || string.IsNullOrEmpty(user.SecurityQuestion))
                return RedirectToPage("./ForgotPassword");

            Question = user.SecurityQuestion;

            // Lưu lại UserId cho post
            TempData["UserId"] = userId;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (TempData["UserId"] == null)
                return RedirectToPage("./ForgotPassword");

            var userId = TempData["UserId"].ToString();
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return RedirectToPage("./ForgotPassword");

            Question = user.SecurityQuestion;

            var passwordHasher = new PasswordHasher<ApplicationUser>();
            var result = passwordHasher.VerifyHashedPassword(user, user.SecurityAnswerHash, Answer);

            // Tạo token reset password
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            if (result == PasswordVerificationResult.Success)
            {
                // Trả về trang reset password, có thể truyền UserId qua query
                return RedirectToPage("./ResetPassword", new { userId = user.Id,token });
            }
            else
            {
                Error = "Incorrect answer. Please try again.";
                TempData["UserId"] = userId; // giữ lại UserId
                return Page();
            }
        }
    }
}
