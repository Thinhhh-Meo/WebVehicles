using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotorcycleShop.Data;
using MotorcycleShop.Models;
using System.Linq;
using System.Threading.Tasks;

namespace MotorcycleShop.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Cart
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var cart = await GetOrCreateCartAsync(userId);

            var cartViewModel = new CartViewModel
            {
                CartId = cart.CartId,
                Items = cart.CartItems.ToList(),
                TotalPrice = cart.CartItems.Sum(ci => ci.Quantity * ci.Product.Price)
            };

            return View(cartViewModel);
        }

        // Helper method to get or create cart
        private async Task<Cart> GetOrCreateCartAsync(string userId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    CreatedDate = DateTime.Now
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();

                // reload để có CartItems
                cart = await _context.Carts
                    .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.Product)
                    .FirstAsync(c => c.CartId == cart.CartId);
            }

            return cart;
        }

        // POST: Cart/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int productId, int quantity = 1)
        {
            var userId = _userManager.GetUserId(User);
            var product = await _context.Products.FindAsync(productId);

            if (product == null || product.Quantity < quantity)
            {
                TempData["ErrorMessage"] = "Product not available or insufficient quantity";
                return RedirectToAction("Details", "Products", new { id = productId });
            }

            var cart = await GetOrCreateCartAsync(userId);

            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CartId == cart.CartId && ci.ProductId == productId);

            if (cartItem == null)
            {
                cartItem = new CartItem
                {
                    CartId = cart.CartId,
                    ProductId = productId,
                    Quantity = quantity
                };
                _context.CartItems.Add(cartItem);
            }
            else
            {
                cartItem.Quantity += quantity;
                _context.Update(cartItem);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Product added to cart";

            return RedirectToAction(nameof(Index));
        }

        // POST: Cart/Update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int cartItemId, int quantity)
        {
            var userId = _userManager.GetUserId(User);
            var cartItem = await _context.CartItems
                .Include(ci => ci.Product)
                .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId && ci.Cart.CartId == _context.Carts.First(c => c.UserId == userId).CartId);

            if (cartItem == null)
            {
                return NotFound();
            }

            if (quantity <= 0)
            {
                _context.CartItems.Remove(cartItem);
            }
            else if (cartItem.Product.Quantity >= quantity)
            {
                cartItem.Quantity = quantity;
                _context.Update(cartItem);
            }
            else
            {
                TempData["ErrorMessage"] = "Insufficient stock";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Cart/Remove
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int cartItemId)
        {
            var userId = _userManager.GetUserId(User);
            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId && ci.Cart.CartId == _context.Carts.First(c => c.UserId == userId).CartId);

            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Cart/GetCartCount
        [HttpGet]
        public async Task<JsonResult> GetCartCount()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(0);
            }

            var userId = _userManager.GetUserId(User);
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                return Json(0);
            }

            var count = cart.CartItems.Sum(ci => ci.Quantity);
            return Json(count);
        }
    }

    public class CartViewModel
    {
        public int CartId { get; set; }
        public List<CartItem> Items { get; set; }
        public decimal TotalPrice { get; set; }
    }
}