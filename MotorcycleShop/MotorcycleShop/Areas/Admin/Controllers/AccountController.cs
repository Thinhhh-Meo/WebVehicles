// Areas/Admin/Controllers/AccountController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MotorcycleShop.Areas.Admin.ViewModels;
using MotorcycleShop.Models;

namespace MotorcycleShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<AccountController> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: /Admin/Account/Login
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Admin/Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                // Find user by email
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user != null)
                {
                    // Check if the user has Admin role
                    var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

                    if (!isAdmin)
                    {
                        ModelState.AddModelError(
                            string.Empty,
                            "You do not have permission to access the admin area."
                        );
                        return View(model);
                    }

                    // Attempt to sign in
                    var result = await _signInManager.PasswordSignInAsync(
                        model.Email,
                        model.Password,
                        model.RememberMe,
                        lockoutOnFailure: false
                    );

                    if (result.Succeeded)
                    {
                        _logger.LogInformation($"Admin {model.Email} has logged in.");

                        // Redirect to Admin Dashboard
                        return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                    }

                    if (result.IsLockedOut)
                    {
                        _logger.LogWarning($"Admin account {model.Email} is locked.");
                        return RedirectToAction("Lockout");
                    }
                }

                ModelState.AddModelError(
                    string.Empty,
                    "Invalid email or password."
                );
            }

            return View(model);
        }

        // POST: /Admin/Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("Admin has logged out.");
            return RedirectToAction("Login", "Account", new { area = "Admin" });
        }

        // GET: /Admin/Account/AccessDenied
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
