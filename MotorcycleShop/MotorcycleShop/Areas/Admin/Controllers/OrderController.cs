using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotorcycleShop.Areas.Admin.ViewModels;
using MotorcycleShop.Data;
using MotorcycleShop.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;


namespace MotorcycleShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            _context = context;
        }

        // GET: /Admin/Order
        public async Task<IActionResult> Index(
            string search,
            string status,
            DateTime? fromDate,
            DateTime? toDate,
            int? page)
        {
            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .ThenInclude(od=>od.Product)
                .OrderByDescending(o => o.OrderId)
                .AsQueryable();

            // Filters
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(o =>
                    o.OrderId.ToString().Contains(search) ||
                    o.User.Email.Contains(search) ||
                    o.User.FullName.Contains(search) ||
                    o.OrderDetails.Any(od => od.Product.Name.Contains(search) ||
                    o.PhoneNumber.Contains(search))
                    );
            }

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<OrderStatus>(status, out OrderStatus orderStatus))
            {
                query = query.Where(o => o.Status == orderStatus);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(o => o.OrderDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(o => o.OrderDate <= toDate.Value.AddDays(1));
            }

            

            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

            // Pagination
            int pageSize = 20;
            int pageNumber = page ?? 1;

            return View(await PaginatedList<Order>.CreateAsync(query, pageNumber, pageSize));
        }

        // GET: /Admin/Order/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .Include(o => o.Discount)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null) return NotFound();

            return View(order);
        }

        // POST: /Admin/Order/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            var oldStatus = order.Status;
            order.Status = status;

            // If cancelled, restore product quantities
            if (status == OrderStatus.Cancelled && oldStatus != OrderStatus.Cancelled)
            {
                var orderDetails = await _context.OrderDetails
                    .Include(od => od.Product)
                    .Where(od => od.OrderId == id)
                    .ToListAsync();

                foreach (var detail in orderDetails)
                {
                    detail.Product.Quantity += detail.Quantity;
                }
            }

            await _context.SaveChangesAsync();

            await LogAdminAction("UpdateStatus", "Order", id,
                $"Changed order #{id} status from {oldStatus} to {status}");

            TempData["Success"] = $"Order #{id} status updated to {status}";
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: /Admin/Order/Statistics
        public async Task<IActionResult> Statistics(int? year)
        {
            var selectedYear = year ?? DateTime.Now.Year;

            var monthlyStats = await _context.Orders
                .Where(o => o.OrderDate.Year == selectedYear)
                .GroupBy(o => o.OrderDate.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    Count = g.Count(),
                    Revenue = g.Sum(o => o.FinalPrice),
                    Average = g.Average(o => o.FinalPrice)
                })
                .OrderBy(g => g.Month)
                .ToListAsync();

            var statusStats = await _context.Orders
                .Where(o => o.OrderDate.Year == selectedYear)
                .GroupBy(o => o.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            var topProducts = await _context.OrderDetails
                .Include(od => od.Product)
                .Where(od => od.Order.OrderDate.Year == selectedYear)
                .GroupBy(od => od.Product.Name)
                .Select(g => new
                {
                    Product = g.Key,
                    Quantity = g.Sum(od => od.Quantity),
                    Revenue = g.Sum(od => od.Quantity * od.UnitPrice)
                })
                .OrderByDescending(g => g.Quantity)
                .Take(10)
                .ToListAsync();

            ViewBag.SelectedYear = selectedYear;
            ViewBag.AvailableYears = await _context.Orders
                .Select(o => o.OrderDate.Year)
                .Distinct()
                .OrderByDescending(y => y)
                .ToListAsync();

            return View(new
            {
                MonthlyStats = monthlyStats,
                StatusStats = statusStats,
                TopProducts = topProducts,
                Year = selectedYear
            });
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