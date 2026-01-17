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
    public class PromotionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public PromotionController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: /Admin/Promotion
        public async Task<IActionResult> Index(string status, int? page)
        {
            var query = _context.Promotions
                .Include(p => p.Discount)
                .AsQueryable();

            var now = DateTime.Now;

            switch (status?.ToLower())
            {
                case "active":
                    query = query.Where(p => p.IsActive &&
                                            p.StartDate <= now &&
                                            p.EndDate >= now);
                    break;
                case "upcoming":
                    query = query.Where(p => p.StartDate > now);
                    break;
                case "expired":
                    query = query.Where(p => p.EndDate < now);
                    break;
                case "inactive":
                    query = query.Where(p => !p.IsActive);
                    break;
            }

            query = query.OrderBy(p => p.DisplayOrder)
                        .ThenByDescending(p => p.StartDate);

            ViewBag.Status = status;

            // Pagination
            int pageSize = 10;
            int pageNumber = page ?? 1;

            return View(await PaginatedList<Promotion>.CreateAsync(query, pageNumber, pageSize));
        }

        // GET: /Admin/Promotion/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Discounts = await _context.Discounts
                .Where(d => d.IsActive && d.DateEnd >= DateTime.Now)
                .Select(d => new { d.DiscountId, d.DiscountName, d.Code })
                .ToListAsync();

            return View();
        }

        // POST: /Admin/Promotion/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Promotion promotion, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                // Handle image upload
                if (imageFile != null && imageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "promotions");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    promotion.ImagePath = $"/images/promotions/{uniqueFileName}";
                }

                _context.Promotions.Add(promotion);
                await _context.SaveChangesAsync();

                await LogAdminAction("Create", "Promotion", promotion.PromotionId,
                    $"Created promotion: {promotion.Condition}");

                TempData["Success"] = "Promotion created successfully!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Discounts = await _context.Discounts
                .Where(d => d.IsActive && d.DateEnd >= DateTime.Now)
                .Select(d => new { d.DiscountId, d.DiscountName, d.Code })
                .ToListAsync();

            return View(promotion);
        }

        // GET: /Admin/Promotion/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion == null) return NotFound();

            ViewBag.Discounts = await _context.Discounts
                .Where(d => d.IsActive && d.DateEnd >= DateTime.Now)
                .Select(d => new { d.DiscountId, d.DiscountName, d.Code })
                .ToListAsync();

            return View(promotion);
        }

        // POST: /Admin/Promotion/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Promotion promotion, IFormFile? imageFile)
        {
            if (id != promotion.PromotionId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingPromotion = await _context.Promotions.FindAsync(id);
                    if (existingPromotion == null) return NotFound();

                    // Update image if new one uploaded
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "promotions");
                        if (!Directory.Exists(uploadsFolder))
                            Directory.CreateDirectory(uploadsFolder);

                        var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(fileStream);
                        }

                        existingPromotion.ImagePath = $"/images/promotions/{uniqueFileName}";
                    }

                    // Update other properties
                    existingPromotion.Condition = promotion.Condition;
                    existingPromotion.Description = promotion.Description;
                    existingPromotion.StartDate = promotion.StartDate;
                    existingPromotion.EndDate = promotion.EndDate;
                    existingPromotion.IsActive = promotion.IsActive;
                    existingPromotion.DisplayOrder = promotion.DisplayOrder;
                    existingPromotion.DiscountId = promotion.DiscountId;

                    _context.Update(existingPromotion);
                    await _context.SaveChangesAsync();

                    await LogAdminAction("Update", "Promotion", id,
                        $"Updated promotion: {promotion.Condition}");

                    TempData["Success"] = "Promotion updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PromotionExists(id))
                        return NotFound();
                    throw;
                }
            }

            ViewBag.Discounts = await _context.Discounts
                .Where(d => d.IsActive && d.DateEnd >= DateTime.Now)
                .Select(d => new { d.DiscountId, d.DiscountName, d.Code })
                .ToListAsync();

            return View(promotion);
        }

        // POST: /Admin/Promotion/ToggleStatus/5
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion == null) return NotFound();

            promotion.IsActive = !promotion.IsActive;
            await _context.SaveChangesAsync();

            var action = promotion.IsActive ? "activated" : "deactivated";
            await LogAdminAction("ToggleStatus", "Promotion", id,
                $"{action} promotion: {promotion.Condition}");

            return Json(new { success = true, isActive = promotion.IsActive });
        }

        // POST: /Admin/Promotion/UpdateOrder
        [HttpPost]
        public async Task<IActionResult> UpdateOrder([FromBody] List<int> promotionIds)
        {
            for (int i = 0; i < promotionIds.Count; i++)
            {
                var promotion = await _context.Promotions.FindAsync(promotionIds[i]);
                if (promotion != null)
                {
                    promotion.DisplayOrder = i + 1;
                }
            }

            await _context.SaveChangesAsync();
            await LogAdminAction("UpdateOrder", "Promotion", null,
                "Updated promotion display order");

            return Json(new { success = true });
        }

        private bool PromotionExists(int id)
        {
            return _context.Promotions.Any(e => e.PromotionId == id);
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