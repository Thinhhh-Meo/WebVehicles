using MotorcycleShop.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MotorcycleShop.Areas.Admin.ViewModels
{
    public class UserWithRolesViewModel
    {
        public ApplicationUser User { get; set; }
        public List<string> Roles { get; set; }
    }

    public class UserIndexViewModel
    {
        public PaginatedList<ApplicationUser> Users { get; set; }
        public List<UserWithRolesViewModel> UserRoles { get; set; }
    }

    public class UserDetailViewModel
    {
        public ApplicationUser User { get; set; }
        public IList<string> Roles { get; set; }
        public List<Order> RecentOrders { get; set; }
        public List<Feedback> RecentFeedbacks { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalSpent { get; set; }
        public double AverageRating { get; set; }
        public string LastLogin { get; set; }
    }

    public class EditRolesViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public List<string> CurrentRoles { get; set; }
        public List<string> AllRoles { get; set; }
        public List<string>? SelectedRoles { get; set; }
    }

    public class CreateUserViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        [StringLength(200)]
        public string? Address { get; set; }

        [Range(18, 100)]
        public int? Age { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }
    }

    public class UserStatisticsViewModel
    {
        public int TotalUsers { get; set; }
        public int NewUsersThisMonth { get; set; }
        public dynamic MonthlyRegistrations { get; set; }
        public List<TopSpender> TopSpenders { get; set; }
    }

    public class TopSpender
    {
        public ApplicationUser User { get; set; }
        public decimal TotalSpent { get; set; }
        public int OrderCount { get; set; }
    }
}