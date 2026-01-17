using Microsoft.AspNetCore.Authorization;
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
    public class DiscountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DiscountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/Discount
        public async Task<IActionResult> Index(string status, int? page)
        {
            var query = _context.Discounts
                .Include(d => d.Orders)
                .AsQueryable();

            switch (status?.ToLower())
            {
                case "active":
                    query = query.Where(d => d.IsActive &&
                                            d.DateStart <= DateTime.Now &&
                                            d.DateEnd >= DateTime.Now);
                    break;
                case "expired":
                    query = query.Where(d => d.DateEnd < DateTime.Now);
                    break;
                case "upcoming":
                    query = query.Where(d => d.DateStart > DateTime.Now);
                    break;
                case "inactive":
                    query = query.Where(d => !d.IsActive);
                    break;
            }

            query = query.OrderByDescending(d => d.DateStart);

            ViewBag.Status = status;

            // Pagination
            int pageSize = 20;
            int pageNumber = page ?? 1;

            return View(await PaginatedList<Discount>.CreateAsync(query, pageNumber, pageSize));
        }

        // GET: /Admin/Discount/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Admin/Discount/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Discount discount)
        {
            if (ModelState.IsValid)
            {
                // Generate unique code if not provided
                if (string.IsNullOrEmpty(discount.Code))
                {
                    discount.Code = GenerateDiscountCode();
                }

                // Set default dates if not provided
                if (discount.DateStart == default)
                {
                    discount.DateStart = DateTime.Now;
                }

                if (discount.DateEnd == default)
                {
                    discount.DateEnd = discount.DateStart.AddMonths(1);
                }

                _context.Discounts.Add(discount);
                await _context.SaveChangesAsync();

                await LogAdminAction("Create", "Discount", discount.DiscountId,
                    $"Created discount: {discount.DiscountName} ({discount.Code})");

                TempData["Success"] = "Discount created successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(discount);
        }

        // GET: /Admin/Discount/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var discount = await _context.Discounts.FindAsync(id);
            if (discount == null) return NotFound();

            return View(discount);
        }

        // POST: /Admin/Discount/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Discount discount)
        {
            if (id != discount.DiscountId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(discount);
                    await _context.SaveChangesAsync();

                    await LogAdminAction("Update", "Discount", id,
                        $"Updated discount: {discount.DiscountName}");

                    TempData["Success"] = "Discount updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DiscountExists(id))
                        return NotFound();
                    throw;
                }
            }

            return View(discount);
        }

        // POST: /Admin/Discount/ToggleStatus/5
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var discount = await _context.Discounts.FindAsync(id);
            if (discount == null) return NotFound();

            discount.IsActive = !discount.IsActive;
            await _context.SaveChangesAsync();

            var action = discount.IsActive ? "activated" : "deactivated";
            await LogAdminAction("ToggleStatus", "Discount", id,
                $"{action} discount: {discount.DiscountName}");

            return Json(new { success = true, isActive = discount.IsActive });
        }

        // GET: /Admin/Discount/GenerateCoupons/5
        public async Task<IActionResult> GenerateCoupons(int id, int quantity)
        {
            var discount = await _context.Discounts.FindAsync(id);
            if (discount == null) return NotFound();

            var coupons = new List<Coupon>();
            for (int i = 0; i < quantity; i++)
            {
                coupons.Add(new Coupon
                {
                    CouponCode = GenerateCouponCode(),
                    DiscountId = id,
                    IsUsed = false
                });
            }

            _context.Coupons.AddRange(coupons);
            await _context.SaveChangesAsync();

            await LogAdminAction("GenerateCoupons", "Discount", id,
                $"Generated {quantity} coupons for discount: {discount.DiscountName}");

            TempData["Success"] = $"Generated {quantity} coupons successfully!";
            return RedirectToAction("Index");
        }

        // GET: /Admin/Discount/UsageReport/5
        public async Task<IActionResult> UsageReport(int id)
        {
            var discount = await _context.Discounts
                .Include(d => d.UserDiscounts)
                    .ThenInclude(ud => ud.User)
                .Include(d => d.Orders)
                .FirstOrDefaultAsync(d => d.DiscountId == id);

            if (discount == null) return NotFound();

            var usageData = await _context.UserDiscounts
                .Include(ud => ud.User)
                .Where(ud => ud.DiscountId == id)
                .OrderByDescending(ud => ud.UsedDate)
                .ToListAsync();

            return View(new
            {
                Discount = discount,
                UsageData = usageData,
                TotalUses = usageData.Count,
                UniqueUsers = usageData.Select(ud => ud.UserId).Distinct().Count()
            });
        }

        private bool DiscountExists(int id)
        {
            return _context.Discounts.Any(e => e.DiscountId == id);
        }

        private string GenerateDiscountCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private string GenerateCouponCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 12)
                .Select(s => s[random.Next(s.Length)]).ToArray());
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