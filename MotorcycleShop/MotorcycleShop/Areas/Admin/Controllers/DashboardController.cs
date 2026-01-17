// Areas/Admin/Controllers/DashboardController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotorcycleShop.Data;
using MotorcycleShop.Areas.Admin.ViewModels;
using MotorcycleShop.Models;

namespace MotorcycleShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(ApplicationDbContext context, ILogger<DashboardController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Admin/Dashboard
        public async Task<IActionResult> Index()
        {
            try
            {
                var today = DateTime.Today;
                var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
                var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                // ===== BASIC COUNTS =====
                var totalUsers = await _context.Users.CountAsync();
                var totalProducts = await _context.Products.CountAsync();
                var totalOrders = await _context.Orders.CountAsync();

                // ✅ TOTAL REVENUE: CHỈ TÍNH DELIVERED
                var totalRevenue = await _context.Orders
                    .Where(o => o.Status == OrderStatus.Delivered)
                    .SumAsync(o => o.FinalPrice);

                // ===== MONTHLY STATS (DELIVERED ONLY) =====
                var monthlyDeliveredOrders = _context.Orders
                    .Where(o =>
                        o.Status == OrderStatus.Delivered &&
                        o.OrderDate >= firstDayOfMonth &&
                        o.OrderDate <= lastDayOfMonth);

                var monthlyOrderCount = await monthlyDeliveredOrders.CountAsync();
                var monthlyRevenue = await monthlyDeliveredOrders.SumAsync(o => o.FinalPrice);

                // ===== ORDER STATUS COUNTS =====
                var pendingOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Pending);
                var confirmedOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Confirmed);
                var shippingOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Shipping);
                var deliveredOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Delivered);
                var cancelledOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Cancelled);

                var model = new DashboardViewModel
                {
                    // Basic stats
                    TotalUsers = totalUsers,
                    TotalProducts = totalProducts,
                    TotalOrders = totalOrders,
                    TotalRevenue = totalRevenue,

                    // Monthly stats
                    MonthlyRevenue = monthlyRevenue,
                    MonthlyOrders = monthlyOrderCount,

                    // Status counts
                    PendingOrders = pendingOrders,
                    ConfirmedOrders = confirmedOrders,
                    ShippingOrders = shippingOrders,
                    DeliveredOrders = deliveredOrders,
                    CancelledOrders = cancelledOrders,

                    // Recent orders (last 7 days – HIỂN THỊ, KHÔNG TÍNH DOANH THU)
                    RecentOrders = await _context.Orders
                        .Include(o => o.User)
                        .Include(o => o.OrderDetails)
                        .Where(o => o.OrderDate >= today.AddDays(-7))
                        .OrderByDescending(o => o.OrderDate)
                        .Take(10)
                        .ToListAsync(),

                    // Low stock products
                    LowStockProducts = await _context.Products
                        .Where(p => p.Quantity < 10)
                        .OrderBy(p => p.Quantity)
                        .Take(10)
                        .ToListAsync(),

                    // Daily stats (DELIVERED ONLY)
                    DailyOrders = await GetDailyOrderStats(30)
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard data");
                return View(new DashboardViewModel());
            }
        }

        [HttpGet]
        public IActionResult ChartData(int days = 30)
        {
            var fromDate = DateTime.Today.AddDays(-days);

            var data = _context.Orders
                .Where(o => o.OrderDate >= fromDate
                         && o.Status == OrderStatus.Delivered)
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new
                {
                    date = g.Key,
                    count = g.Count(),
                    revenue = g.Sum(x => x.FinalPrice)
                })
                .OrderBy(x => x.date)
                .ToList();

            return Json(new
            {
                success = true,
                daily = data
            });
        }


        // API: /Admin/Dashboard/Stats
        [HttpGet]
        public async Task<IActionResult> Stats()
        {
            try
            {
                var today = DateTime.Today;
                var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
                var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                var totalRevenue = await _context.Orders
                    .Where(o => o.Status == OrderStatus.Delivered)
                    .SumAsync(o => o.FinalPrice);

                var monthlyRevenue = await _context.Orders
                    .Where(o =>
                        o.Status == OrderStatus.Delivered &&
                        o.OrderDate >= firstDayOfMonth &&
                        o.OrderDate <= lastDayOfMonth)
                    .SumAsync(o => o.FinalPrice);

                var stats = new
                {
                    totalUsers = await _context.Users.CountAsync(),
                    totalProducts = await _context.Products.CountAsync(),
                    totalOrders = await _context.Orders.CountAsync(),
                    totalRevenue,
                    monthlyRevenue,
                    monthlyOrders = await _context.Orders
                        .CountAsync(o =>
                            o.Status == OrderStatus.Delivered &&
                            o.OrderDate >= firstDayOfMonth &&
                            o.OrderDate <= lastDayOfMonth),

                    pendingOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Pending),
                    confirmedOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Confirmed),
                    shippingOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Shipping),
                    deliveredOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Delivered),
                    cancelledOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Cancelled),
                    lowStockProducts = await _context.Products.CountAsync(p => p.Quantity < 10),
                    dailyOrders = await GetDailyOrderStats(7)
                };

                return Json(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard stats");
                return Json(new { success = false, message = ex.Message });
            }
        }

        private async Task<List<DailyOrder>> GetDailyOrderStats(int days)
        {
            var endDate = DateTime.Today;
            var startDate = endDate.AddDays(-days);

            // ✅ CHỈ LẤY ORDER DELIVERED
            var orders = await _context.Orders
                .Where(o =>
                    o.Status == OrderStatus.Delivered &&
                    o.OrderDate >= startDate &&
                    o.OrderDate <= endDate)
                .ToListAsync();

            var dailyStats = orders
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new DailyOrder
                {
                    Date = g.Key,
                    Count = g.Count(),
                    Revenue = g.Sum(o => o.FinalPrice)
                })
                .OrderBy(d => d.Date)
                .ToList();

            var result = new List<DailyOrder>();
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var stat = dailyStats.FirstOrDefault(d => d.Date.Date == date.Date);
                result.Add(stat ?? new DailyOrder
                {
                    Date = date,
                    Count = 0,
                    Revenue = 0
                });
            }

            return result;
        }
    }
}
