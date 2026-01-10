using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotorcycleShop.Data;
using MotorcycleShop.Models;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MotorcycleShop.Models.ViewModels;

namespace MotorcycleShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Get featured products
            var featuredProducts = await _context.Products
                .Where(p => p.IsActive && p.Quantity > 0)
                .OrderByDescending(p => p.CreatedDate)
                .Take(8)
                .ToListAsync();

            // Get new arrivals
            var newArrivals = await _context.Products
                .Where(p => p.IsActive && p.Quantity > 0)
                .OrderByDescending(p => p.CreatedDate)
                .Take(4)
                .ToListAsync();

            // Get brands
            var brands = await _context.Brands
                .Where(b => b.IsActive)
                .Take(6)
                .ToListAsync();

            ViewBag.FeaturedProducts = featuredProducts;
            ViewBag.NewArrivals = newArrivals;
            ViewBag.Brands = brands;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}