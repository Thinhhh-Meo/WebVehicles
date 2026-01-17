using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotorcycleShop.Areas.Admin.ViewModels;
using MotorcycleShop.Data;
using MotorcycleShop.Models;
using System.Linq;
using System.Threading.Tasks;

namespace MotorcycleShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: /Admin/User
        public async Task<IActionResult> Index(string search, string role, int? page)
        {
            var query = _userManager.Users.AsQueryable();

            // Search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u =>
                    u.UserName.Contains(search) ||
                    u.Email.Contains(search) ||
                    u.FullName.Contains(search) ||
                    u.PhoneNumber.Contains(search));
            }

            // Role filter
            if (!string.IsNullOrEmpty(role))
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(role);
                var userIds = usersInRole.Select(u => u.Id);
                query = query.Where(u => userIds.Contains(u.Id));
            }

            query = query.OrderBy(u => u.UserName);

            ViewBag.Search = search;
            ViewBag.Role = role;
            ViewBag.AllRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();

            // Pagination
            int pageSize = 20;
            int pageNumber = page ?? 1;

            return View(await PaginatedList<ApplicationUser>.CreateAsync(query, pageNumber, pageSize));
        }

        // GET: /Admin/User/Details/5
        public async Task<IActionResult> Details(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Get user roles
            var roles = await _userManager.GetRolesAsync(user);

            // Get user statistics
            var orders = await _context.Orders
                .Where(o => o.UserId == id)
                .OrderByDescending(o => o.OrderDate)
                .Take(10)
                .ToListAsync();

            var feedbacks = await _context.Feedbacks
                .Include(f => f.Product)
                .Where(f => f.UserId == id)
                .OrderByDescending(f => f.CreatedDate)
                .Take(10)
                .ToListAsync();

            var totalSpent = await _context.Orders
                .Where(o => o.UserId == id)
                .SumAsync(o => o.FinalPrice);

            return View(new
            {
                User = user,
                Roles = roles,
                RecentOrders = orders,
                RecentFeedbacks = feedbacks,
                OrderCount = await _context.Orders.CountAsync(o => o.UserId == id),
                TotalSpent = totalSpent,
                AverageRating = feedbacks.Any() ? feedbacks.Average(f => f.Rating) : 0,
                LastLogin = user.LockoutEnd?.ToString("yyyy-MM-dd HH:mm") ?? "N/A"
            });
        }

        // GET: /Admin/User/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            return View(user);
        }

        // POST: /Admin/User/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, ApplicationUser model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null) return NotFound();

                // Update user properties
                user.FullName = model.FullName;
                user.Email = model.Email;
                user.PhoneNumber = model.PhoneNumber;
                user.Address = model.Address;
                user.Age = model.Age;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    await LogAdminAction("Update", "User", 0, $"Updated user: {user.Email}");
                    TempData["Success"] = "User updated successfully!";
                    return RedirectToAction(nameof(Details), new { id });
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        // POST: /Admin/User/ToggleLock/5
        [HttpPost]
        public async Task<IActionResult> ToggleLock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (user.LockoutEnd == null || user.LockoutEnd < DateTime.Now)
            {
                // Lock account for 30 days
                user.LockoutEnd = DateTime.Now.AddDays(30);
                await LogAdminAction("Lock", "User", 0, $"Locked user: {user.Email}");
                TempData["Success"] = "Account locked for 30 days";
            }
            else
            {
                // Unlock account
                user.LockoutEnd = null;
                await LogAdminAction("Unlock", "User", 0, $"Unlocked user: {user.Email}");
                TempData["Success"] = "Account unlocked";
            }

            await _userManager.UpdateAsync(user);
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: /Admin/User/EditRoles/5
        public async Task<IActionResult> EditRoles(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = await _roleManager.Roles.ToListAsync();

            return View(new
            {
                UserId = user.Id,
                UserName = user.UserName,
                UserEmail = user.Email,
                CurrentRoles = userRoles.ToList(),
                AllRoles = allRoles.Select(r => r.Name).ToList()
            });
        }

        // POST: /Admin/User/UpdateRoles
        [HttpPost]
        public async Task<IActionResult> UpdateRoles(string userId, List<string> selectedRoles)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            // Get current roles
            var currentRoles = await _userManager.GetRolesAsync(user);

            // Calculate roles to add and remove
            var rolesToAdd = selectedRoles?.Except(currentRoles).ToList() ?? new List<string>();
            var rolesToRemove = currentRoles.Except(selectedRoles ?? new List<string>()).ToList();

            // Remove roles
            if (rolesToRemove.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            }

            // Add roles
            if (rolesToAdd.Any())
            {
                await _userManager.AddToRolesAsync(user, rolesToAdd);
            }

            await LogAdminAction("UpdateRoles", "User", 0,
                $"Updated roles for {user.Email}. Added: {string.Join(", ", rolesToAdd)}, Removed: {string.Join(", ", rolesToRemove)}");

            TempData["Success"] = "User roles updated successfully!";
            return RedirectToAction(nameof(Details), new { id = userId });
        }

        // GET: /Admin/User/Statistics
        public async Task<IActionResult> Statistics()
        {
            var totalUsers = await _userManager.Users.CountAsync();
            var newUsersThisMonth = await _userManager.Users
                .Where(u => u.CreatedDate >= new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1))
                .CountAsync();

            // User registration trend (last 6 months)
            var monthlyRegistrations = await _userManager.Users
                .Where(u => u.CreatedDate >= DateTime.Now.AddMonths(-6))
                .GroupBy(u => new { u.CreatedDate.Year, u.CreatedDate.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Count = g.Count()
                })
                .OrderBy(g => g.Year)
                .ThenBy(g => g.Month)
                .ToListAsync();

            // Top spending users
            var topSpenders = await _context.Orders
                .Include(o => o.User)
                .Where(o => o.OrderDate >= DateTime.Now.AddMonths(-3))
                .GroupBy(o => o.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    TotalSpent = g.Sum(o => o.FinalPrice),
                    OrderCount = g.Count()
                })
                .OrderByDescending(g => g.TotalSpent)
                .Take(10)
                .Join(_userManager.Users,
                    spending => spending.UserId,
                    user => user.Id,
                    (spending, user) => new
                    {
                        User = user,
                        TotalSpent = spending.TotalSpent,
                        OrderCount = spending.OrderCount
                    })
                .ToListAsync();

            return View(new
            {
                TotalUsers = totalUsers,
                NewUsersThisMonth = newUsersThisMonth,
                MonthlyRegistrations = monthlyRegistrations,
                TopSpenders = topSpenders
            });
        }

        private async Task LogAdminAction(string action, string entity, int? entityId, string description)
        {
            var log = new MotorcycleShop.Models.Admin.AdminLog
            {
                AdminId = User.Identity.Name,
                Action = action,
                Entity = entity,
                EntityId = entityId,
                Description = description,
                IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Timestamp = DateTime.Now
            };
            _context.AdminLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}