using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MotorcycleShop.Models;

namespace MotorcycleShop.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var passwordHasher = new PasswordHasher<ApplicationUser>();


            // Seed roles
            string[] roleNames = { "Admin", "Customer" };
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Seed admin user
            var adminEmail = "admin@motorshop.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Admin",
                    PhoneNumber = "0909123456",
                    Address = "123 Admin Street",
                    EmailConfirmed = true,
                    AvatarUrl = "/images/avatars/default.png",
                    SecurityQuestion = "soccer"
                };
                // Hash câu trả lời bảo mật
                adminUser.SecurityAnswerHash =
                    passwordHasher.HashPassword(adminUser, "adminsoccer");

                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Seed sample customer
            var customerEmail = "customer@motorshop.com";
            var customerUser = await userManager.FindByEmailAsync(customerEmail);
            if (customerUser == null)
            {
                customerUser = new ApplicationUser
                {
                    UserName = customerEmail,
                    Email = customerEmail,
                    FullName = "Sample Customer",
                    PhoneNumber = "0909876543",
                    Address = "456 Customer Street",
                    EmailConfirmed = true,
                    AvatarUrl = "/images/avatars/default.png",
                    SecurityQuestion = "dream"
                };
                // Hash câu trả lời bảo mật
                customerUser.SecurityAnswerHash =
                    passwordHasher.HashPassword(customerUser, "rich");

                var result = await userManager.CreateAsync(customerUser, "Customer@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(customerUser, "Customer");
                }
            }
        }
    }
}