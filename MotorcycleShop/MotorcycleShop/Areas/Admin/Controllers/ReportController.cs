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
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReportController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Admin/Report
        public IActionResult Index()
        {
            return View();
        }

        // GET: /Admin/Report/Sales
        public async Task<IActionResult> Sales(DateTime? startDate, DateTime? endDate,OrderStatus? status)
        {
            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                     .Where(o => o.Status != OrderStatus.Cancelled)
                .AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(o => o.OrderDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(o => o.OrderDate <= endDate.Value.AddDays(1));
            }
            // Filter status 
            if (status.HasValue)
                query = query.Where(o => o.Status == status.Value);

            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
            var deliveredOrders = orders
                .Where(o => o.Status == OrderStatus.Delivered)
                .ToList();

            var vm = new SalesReportViewModel
            {
                Orders = orders,
                TotalOrders = deliveredOrders.Count,
                TotalRevenue = deliveredOrders.Sum(o => o.FinalPrice),
                AverageOrderValue = deliveredOrders.Any() ? deliveredOrders.Average(o => o.FinalPrice) : 0,
                StartDate = startDate,
                EndDate = endDate,
                SelectedStatus = status
            };

            return View(vm);
        }

        // GET: /Admin/Report/Inventory
        public async Task<IActionResult> Inventory()
        {
            var products = await _context.Products
                .Include(p => p.Brand)
                .OrderByDescending(p => p.Quantity)
                .ToListAsync();

            var reportData = new
            {
                Products = products,
                TotalProducts = products.Count,
                TotalStockValue = products.Sum(p => p.Quantity * p.Price),
                LowStockProducts = products.Where(p => p.Quantity <= 5).ToList(),
                OutOfStockProducts = products.Where(p => p.Quantity == 0).ToList()
            };

            return View(reportData);
        }

        // GET: /Admin/Report/Customer
        public async Task<IActionResult> Customer()
        {
            var users = await _context.Users
                .Include(u => u.Orders)
                .Where(u => u.Orders.Any())
                .Select(u => new
                {
                    User = u,
                    OrderCount = u.Orders.Count,
                    TotalSpent = u.Orders.Sum(o => o.FinalPrice),
                    LastOrderDate = u.Orders.Max(o => o.OrderDate)
                })
                .OrderByDescending(u => u.TotalSpent)
                .ToListAsync();

            return View(users);
        }

        // POST: /Admin/Report/Generate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Generate(string reportType, DateTime? startDate, DateTime? endDate)
        {
            var userId = _userManager.GetUserId(User);
            var report = new Report
            {
                ReportType = reportType,
                ReportDate = DateTime.Now,
                GeneratedByUserId = userId,
                Description = $"{reportType} Report from {startDate?.ToString("yyyy-MM-dd") ?? "beginning"} to {endDate?.ToString("yyyy-MM-dd") ?? "now"}"
            };

            // Generate report data based on type
            switch (reportType)
            {
                case "Sales":
                    var salesData = await GetSalesReportData(startDate, endDate);
                    report.DataJson = System.Text.Json.JsonSerializer.Serialize(salesData);
                    break;
                case "Inventory":
                    var inventoryData = await GetInventoryReportData();
                    report.DataJson = System.Text.Json.JsonSerializer.Serialize(inventoryData);
                    break;
                case "Customer":
                    var customerData = await GetCustomerReportData();
                    report.DataJson = System.Text.Json.JsonSerializer.Serialize(customerData);
                    break;
            }

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            await LogAdminAction("Generate", "Report", report.ReportId,
                $"Generated {reportType} report");

            TempData["Success"] = $"Report generated successfully (ID: {report.ReportId})";
            return RedirectToAction(nameof(Index));
        }

        private async Task<object> GetSalesReportData(DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Orders
                .Where(o => o.Status != OrderStatus.Cancelled)
                .AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(o => o.OrderDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(o => o.OrderDate <= endDate.Value);
            }

            var orders = await query.ToListAsync();

            return new
            {
                TotalOrders = orders.Count,
                TotalRevenue = orders.Sum(o => o.FinalPrice),
                AverageOrderValue = orders.Any() ? orders.Average(o => o.FinalPrice) : 0,
                OrdersByStatus = orders.GroupBy(o => o.Status)
                    .Select(g => new { Status = g.Key, Count = g.Count() }),
                MonthlyRevenue = orders.GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                    .Select(g => new
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        Revenue = g.Sum(o => o.FinalPrice),
                        Count = g.Count()
                    })
            };
        }

        private async Task<object> GetInventoryReportData()
        {
            var products = await _context.Products
                .Include(p => p.Brand)
                .ToListAsync();

            return new
            {
                TotalProducts = products.Count,
                TotalStockValue = products.Sum(p => p.Quantity * p.Price),
                ProductsByType = products.GroupBy(p => p.TypeProduct)
                    .Select(g => new { Type = g.Key, Count = g.Count() }),
                LowStockProducts = products.Where(p => p.Quantity <= 5).Count(),
                OutOfStockProducts = products.Where(p => p.Quantity == 0).Count()
            };
        }

        private async Task<object> GetCustomerReportData()
        {
            var users = await _context.Users
                .Include(u => u.Orders)
                .Where(u => u.Orders.Any())
                .Select(u => new
                {
                    UserId = u.Id,
                    UserName = u.UserName,
                    FullName = u.FullName,
                    Email = u.Email,
                    OrderCount = u.Orders.Count,
                    TotalSpent = u.Orders.Sum(o => o.FinalPrice),
                    LastOrderDate = u.Orders.Max(o => o.OrderDate)
                })
                .ToListAsync();

            return new
            {
                TotalCustomers = users.Count,
                AverageOrdersPerCustomer = users.Any() ? users.Average(u => u.OrderCount) : 0,
                AverageSpendingPerCustomer = users.Any() ? users.Average(u => u.TotalSpent) : 0,
                TopCustomers = users.OrderByDescending(u => u.TotalSpent).Take(10)
            };
        }

        private async Task LogAdminAction(string action, string entity, int? entityId, string description)
        {
            var adminId = _userManager.GetUserId(User);
            var log = new MotorcycleShop.Models.Admin.AdminLog
            {
                AdminId = adminId,
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