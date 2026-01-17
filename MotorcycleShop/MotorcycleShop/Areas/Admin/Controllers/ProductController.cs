using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly UserManager<ApplicationUser> _userManager;


        public ProductController(ApplicationDbContext context, IWebHostEnvironment environment, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _environment = environment;
            _userManager = userManager;
        }

        // GET: /Admin/Product
        public async Task<IActionResult> Index(string search, string type, string brand, int? page)
        {
            var query = _context.Products
                .Include(p => p.Brand)
                .AsQueryable();

            // Filters
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name.Contains(search) ||
                                        p.Description.Contains(search));
            }

            if (!string.IsNullOrEmpty(type) && Enum.TryParse<ProductType>(type, out ProductType productType))
            {
                query = query.Where(p => p.TypeProduct == productType);
            }

            if (!string.IsNullOrEmpty(brand))
            {
                query = query.Where(p => p.BrandName == brand);
            }

            query = query.OrderByDescending(p => p.CreatedDate);

            ViewBag.Search = search;
            ViewBag.Type = type;
            ViewBag.BrandFilter = brand;
            ViewBag.Brands = await _context.Brands
                .Where(b => b.IsActive)
                .Select(b => b.Name)
                .Distinct()
                .ToListAsync();

            // Pagination
            int pageSize = 15;
            int pageNumber = page ?? 1;

            return View(await PaginatedList<Product>.CreateAsync(query, pageNumber, pageSize));
        }

        // GET: /Admin/Product/Create
        public async Task<IActionResult> Create()
        {
            var vm = new CreateProductViewModel
            {
                Brands = await _context.Brands
            .Where(b => b.IsActive)
            .Select(b => new SelectListItem
            {
                Value = b.BrandId.ToString(),
                Text = b.Name
            })
            .ToListAsync()
            };

            return View(vm);
        }

        // POST: /Admin/Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateProductViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Brands = await LoadBrands();
                return View(vm);
            }

            if (ModelState.IsValid)
            {
                var product = new Product
                {
                    Name = vm.Name,
                    Description = vm.Description,
                    Price = vm.Price,
                    Quantity = vm.Quantity,
                    BrandId = vm.BrandId,
                    TypeProduct = vm.TypeProduct,
                    IsActive = vm.IsActive
                };

                // Handle image upload
                if (vm.ImageFile != null && vm.ImageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "products");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(vm.ImageFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await vm.ImageFile.CopyToAsync(fileStream);
                    }

                    product.ImageUrl = $"/images/products/" + uniqueFileName;
                }
                else
                {
                    product.ImageUrl = "/images/products/default.jpg";
                }

                // BRAND NAME
                product.BrandName = await _context.Brands
                    .Where(b => b.BrandId == vm.BrandId)
                    .Select(b => b.Name)
                    .FirstOrDefaultAsync();

                _context.Products.Add(product);
                await _context.SaveChangesAsync();






                // Create Motorcycle or SparePart based on type
                if (vm.TypeProduct == ProductType.Motorcycle)
                {
                    if (vm.Motorcycle == null
                        || vm.Motorcycle.TypeVehicle == null
                        || vm.Motorcycle.Displacement == null   
                        || vm.Motorcycle.FuelCapacity == null
                        || vm.Motorcycle.Weight == null)
                    {
                        ModelState.AddModelError("", "Please enter all motorcycle details");
                        vm.Brands = await LoadBrands();
                        return View(vm);
                    }

                    _context.Motorcycles.Add(new Motorcycle
                    {
                        ProductId = product.ProductId,
                        VehicleName = product.Name,
                        TypeVehicle = vm.Motorcycle.TypeVehicle.Value,
                        Displacement = vm.Motorcycle.Displacement.Value,
                        FuelCapacity = vm.Motorcycle.FuelCapacity.Value,
                        Weight = vm.Motorcycle.Weight.Value,
                        Color = vm.Motorcycle.Color
                    });
                    
                }
                else if (vm.TypeProduct == ProductType.SparePart && vm.SparePart !=null)
                {
                    _context.SpareParts.Add(new SparePart
                    {
                        ProductId = product.ProductId,
                        SpareName = product.Name,
                        Category = vm.SparePart.Category,
                        CompatibleWith = vm.SparePart.CompatibleWith
                    });
                   
                }
                if (vm.TypeProduct == ProductType.Motorcycle && vm.Motorcycle == null)
                {
                    ModelState.AddModelError("", "Please enter motorcycle details");
                }

                if (vm.TypeProduct == ProductType.SparePart && vm.SparePart == null)
                {
                    ModelState.AddModelError("", "Please enter spare part details");
                }

                await _context.SaveChangesAsync();


                // Log action
                await LogAdminAction("Create", "Product", product.ProductId,
                    $"Created product: {product.Name}");

                TempData["Success"] = "Product created successfully!";
                return RedirectToAction(nameof(Index));

            }

            

            vm.Brands = await _context.Brands
           .Where(b => b.IsActive)
           .Select(b => new SelectListItem
           {
               Value = b.BrandId.ToString(),
               Text = b.Name
           })
           .ToListAsync();

            return View(vm);
        }

        // GET: /Admin/Product/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products
                .Include(p => p.Motorcycle)
                .Include(p => p.SparePart)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null) return NotFound();

            ViewBag.Brands = await _context.Brands
                .Where(b => b.IsActive)
                 .Select(b => new SelectListItem
                 {
                     Value = b.BrandId.ToString(),
                     Text = b.Name
                 })
                .ToListAsync();

            return View(product);
        }

        // POST: /Admin/Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile? imageFile)
        {
            if (id != product.ProductId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingProduct = await _context.Products.FindAsync(id);
                    if (existingProduct == null) return NotFound();

                    // Update image if new one uploaded
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "products");
                        if (!Directory.Exists(uploadsFolder))
                            Directory.CreateDirectory(uploadsFolder);

                        var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(fileStream);
                        }

                        existingProduct.ImageUrl = $"/images/products/{uniqueFileName}";
                    }

                    // Update other properties
                    existingProduct.Name = product.Name;
                    existingProduct.Description = product.Description;
                    existingProduct.Price = product.Price;
                    existingProduct.Quantity = product.Quantity;
                    existingProduct.BrandId = product.BrandId;
                    existingProduct.BrandName = await _context.Brands
                        .Where(b => b.BrandId == product.BrandId)
                        .Select(b => b.Name)
                        .FirstOrDefaultAsync();
                    existingProduct.TypeProduct = product.TypeProduct;
                    existingProduct.IsActive = product.IsActive;

                    _context.Update(existingProduct);

                    // Update Motorcycle or SparePart if type changed
                    if (product.TypeProduct == ProductType.Motorcycle)
                    {
                        var motorcycle = await _context.Motorcycles
                            .FirstOrDefaultAsync(m => m.ProductId == id);

                        if (motorcycle == null)
                        {
                            motorcycle = new Motorcycle
                            {
                                ProductId = id,
                                VehicleName = product.Name,
                                TypeVehicle = VehicleType.Sport,
                                Displacement = 150,
                                FuelCapacity = 10.0f,
                                Weight = 150,
                                Color = "Black"
                            };
                            _context.Motorcycles.Add(motorcycle);
                        }
                        // Remove SparePart if exists
                        var sparePart = await _context.SpareParts
                            .FirstOrDefaultAsync(s => s.ProductId == id);
                        if (sparePart != null)
                        {
                            _context.SpareParts.Remove(sparePart);
                        }
                    }
                    else if (product.TypeProduct == ProductType.SparePart)
                    {
                        var sparePart = await _context.SpareParts
                            .FirstOrDefaultAsync(s => s.ProductId == id);

                        if (sparePart == null)
                        {
                            sparePart = new SparePart
                            {
                                ProductId = id,
                                SpareName = product.Name,
                                Category = "General",
                                CompatibleWith = "Multiple models"
                            };
                            _context.SpareParts.Add(sparePart);
                        }
                        // Remove Motorcycle if exists
                        var motorcycle = await _context.Motorcycles
                            .FirstOrDefaultAsync(m => m.ProductId == id);
                        if (motorcycle != null)
                        {
                            _context.Motorcycles.Remove(motorcycle);
                        }
                    }

                    await _context.SaveChangesAsync();

                    await LogAdminAction("Update", "Product", id,
                        $"Updated product: {product.Name}");

                    TempData["Success"] = "Product updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(id))
                        return NotFound();
                    throw;
                }
            }

            ViewBag.Brands = await _context.Brands
                .Where(b => b.IsActive)
                .Select(b => new SelectListItem{
                    Value = b.BrandId.ToString(),
                    Text = b.Name
                })
                .ToListAsync();
            return View(product);
        }

        // POST: /Admin/Product/ToggleStatus/5
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            product.IsActive = !product.IsActive;
            await _context.SaveChangesAsync();

            await LogAdminAction("ToggleStatus", "Product", id,
                $"{(product.IsActive ? "Activated" : "Deactivated")} product: {product.Name}");

            return Json(new { success = true, isActive = product.IsActive });
        }

        // GET: /Admin/Product/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Motorcycle)
                .Include(p => p.SparePart)
                .Include(p => p.Feedbacks)
                    .ThenInclude(f => f.User)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null) return NotFound();

            return View(product);
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }

        private async Task LogAdminAction(string action, string entity, int? entityId, string description)
        {
            var adminId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(adminId))
            {
                throw new Exception("AdminId is null - user not authenticated");
            }
            var log = new MotorcycleShop.Models.Admin.AdminLog
            {
                AdminId =adminId,
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
        private async Task<List<SelectListItem>> LoadBrands()
        {
            return await _context.Brands    
                .Where(b => b.IsActive)
                .Select(b => new SelectListItem
                {
                    Value = b.BrandId.ToString(),
                    Text = b.Name
                })
                .ToListAsync();
        }

    }
}