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
    public class BrandController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public BrandController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: /Admin/Brand
        public async Task<IActionResult> Index(string search, int? page)
        {
            var query = _context.Brands.AsQueryable();

            // Search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(b => b.Name.Contains(search) ||
                                        b.Country.Contains(search) ||
                                        b.Description.Contains(search));
            }

            query = query.OrderBy(b => b.Name);

            ViewBag.Search = search;

            // Pagination
            int pageSize = 15;
            int pageNumber = page ?? 1;

            return View(await PaginatedList<Brand>.CreateAsync(query, pageNumber, pageSize));
        }

        // GET: /Admin/Brand/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Admin/Brand/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Brand brand, IFormFile? logoFile)
        {
            if (ModelState.IsValid)
            {
                // Handle logo upload
                if (logoFile != null && logoFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "brands");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(logoFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await logoFile.CopyToAsync(fileStream);
                    }

                    brand.LogoUrl = $"/images/brands/{uniqueFileName}";
                }

                _context.Brands.Add(brand);
                await _context.SaveChangesAsync();

                await LogAdminAction("Create", "Brand", brand.BrandId,
                    $"Created brand: {brand.Name}");

                TempData["Success"] = "Brand created successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(brand);
        }

        // GET: /Admin/Brand/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null) return NotFound();

            return View(brand);
        }

        // POST: /Admin/Brand/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Brand brand, IFormFile? logoFile)
        {
            if (id != brand.BrandId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingBrand = await _context.Brands.FindAsync(id);
                    if (existingBrand == null) return NotFound();

                    // Update logo if new one uploaded
                    if (logoFile != null && logoFile.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "brands");
                        if (!Directory.Exists(uploadsFolder))
                            Directory.CreateDirectory(uploadsFolder);

                        var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(logoFile.FileName);
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await logoFile.CopyToAsync(fileStream);
                        }

                        existingBrand.LogoUrl = $"/images/brands/{uniqueFileName}";
                    }

                    // Update other properties
                    existingBrand.Name = brand.Name;
                    existingBrand.Description = brand.Description;
                    existingBrand.Country = brand.Country;
                    existingBrand.EstablishedYear = brand.EstablishedYear;
                    existingBrand.IsActive = brand.IsActive;

                    _context.Update(existingBrand);
                    await _context.SaveChangesAsync();

                    await LogAdminAction("Update", "Brand", id,
                        $"Updated brand: {brand.Name}");

                    TempData["Success"] = "Brand updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BrandExists(id))
                        return NotFound();
                    throw;
                }
            }

            return View(brand);
        }

        // POST: /Admin/Brand/ToggleStatus/5
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null) return NotFound();

            brand.IsActive = !brand.IsActive;
            await _context.SaveChangesAsync();

            var action = brand.IsActive ? "activated" : "deactivated";
            await LogAdminAction("ToggleStatus", "Brand", id,
                $"{action} brand: {brand.Name}");

            return Json(new { success = true, isActive = brand.IsActive });
        }

        // GET: /Admin/Brand/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var brand = await _context.Brands
                .Include(b => b.Products)
                .FirstOrDefaultAsync(b => b.BrandId == id);

            if (brand == null) return NotFound();

            return View(brand);
        }

        private bool BrandExists(int id)
        {
            return _context.Brands.Any(e => e.BrandId == id);
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