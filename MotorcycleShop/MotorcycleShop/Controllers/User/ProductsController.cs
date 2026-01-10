using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotorcycleShop.Data;
using MotorcycleShop.Models;
using System.Linq;
using System.Threading.Tasks;

namespace MotorcycleShop.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Products
        public async Task<IActionResult> Index(string search, string category, string brand, int? page)
        {
            var products = _context.Products
                .Include(p => p.Brand)
                .Where(p => p.IsActive && p.Quantity > 0);

            // Filter by search
            if (!string.IsNullOrEmpty(search))
            {
                products = products.Where(p =>
                    p.Name.Contains(search) ||
                    p.Description.Contains(search) ||
                    p.BrandName.Contains(search));
            }

            // Filter by category
            if (!string.IsNullOrEmpty(category))
            {
                if (category == "Motorcycle")
                {
                    products = products.Where(p => p.TypeProduct == ProductType.Motorcycle);
                }
                else if (category == "SparePart")
                {
                    products = products.Where(p => p.TypeProduct == ProductType.SparePart);
                }
            }

            // Filter by brand
            if (!string.IsNullOrEmpty(brand))
            {
                products = products.Where(p => p.BrandName == brand);
            }

            // Get distinct brands for filter
            ViewBag.Brands = await _context.Products
                .Where(p => p.IsActive)
                .Select(p => p.BrandName)
                .Distinct()
                .ToListAsync();

            int pageSize = 12;
            int pageNumber = page ?? 1;

            var productList = await products.ToListAsync();
            return View(productList);
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Motorcycle)
                .Include(p => p.SparePart)
                .Include(p => p.Feedbacks)
                    .ThenInclude(f => f.User)
                .FirstOrDefaultAsync(m => m.ProductId == id);

            if (product == null || !product.IsActive)
            {
                return NotFound();
            }

            // Get related products
            var relatedProducts = await _context.Products
                .Where(p => p.BrandName == product.BrandName &&
                           p.ProductId != product.ProductId &&
                           p.IsActive)
                .Take(4)
                .ToListAsync();

            ViewBag.RelatedProducts = relatedProducts;

            return View(product);
        }
    }
}