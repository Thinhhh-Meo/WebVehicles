using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotorcycleShop.Data;
using MotorcycleShop.Models;
using System.Linq;
using System.Threading.Tasks;

namespace MotorcycleShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin
        public async Task<IActionResult> Dashboard()
        {
            var today = DateTime.Today;
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var lastMonthStart = monthStart.AddMonths(-1);

            var dashboard = new
            {
                // Statistics
                TotalUsers = await _context.Users.CountAsync(),
                TotalProducts = await _context.Products.CountAsync(p => p.IsActive),
                TotalOrders = await _context.Orders.CountAsync(),
                TotalRevenue = await _context.Orders.SumAsync(o => o.FinalPrice),

                // Monthly Stats
                MonthlyRevenue = await _context.Orders
                    .Where(o => o.OrderDate >= monthStart && o.OrderDate <= today)
                    .SumAsync(o => o.FinalPrice),

                MonthlyOrders = await _context.Orders
                    .Where(o => o.OrderDate >= monthStart && o.OrderDate <= today)
                    .CountAsync(),

                // Recent Data
                RecentOrders = await _context.Orders
                    .Include(o => o.User)
                    .OrderByDescending(o => o.OrderDate)
                    .Take(10)
                    .ToListAsync(),

                LowStockProducts = await _context.Products
                    .Where(p => p.Quantity <= 5 && p.IsActive)
                    .OrderBy(p => p.Quantity)
                    .Take(10)
                    .ToListAsync(),

                RecentActivities = await _context.AdminLogs
                    .Include(l => l.Admin)
                    .OrderByDescending(l => l.Timestamp)
                    .Take(20)
                    .ToListAsync()
            };

            return View(dashboard);
        }
    }
}