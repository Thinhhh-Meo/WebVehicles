using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotorcycleShop.Data;
using MotorcycleShop.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
namespace MotorcycleShop.Controllers
{
    [Authorize]
    public class DiscountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DiscountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Discount/Available
        public async Task<IActionResult> Available()
        {
            var discounts = await _context.Discounts
                .Where(d => d.IsActive &&
                           d.DateStart <= DateTime.Now &&
                           d.DateEnd >= DateTime.Now &&
                           d.UsedCount < (d.UsageLimit ?? int.MaxValue))
                .OrderByDescending(d => d.DateEnd)
                .ToListAsync();

            return View(discounts);
        }

        // GET: Discount/Validate
        [HttpGet]
        public async Task<IActionResult> Validate(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return Json(new { valid = false, message = "Please enter a discount code" });
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var discount = await _context.Discounts
                .FirstOrDefaultAsync(d => d.Code == code &&
                                         d.IsActive &&
                                         d.DateStart <= DateTime.Now &&
                                         d.DateEnd >= DateTime.Now &&
                                         d.UsedCount < (d.UsageLimit ?? int.MaxValue));

            if (discount == null)
            {
                return Json(new { valid = false, message = "Invalid or expired discount code" });
            }

            // 🚫 CHECK USER ĐÃ DÙNG CHƯA
            bool alreadyUsed = await _context.UserDiscounts
                .AnyAsync(ud => ud.UserId == userId && ud.DiscountId == discount.DiscountId);

            if (alreadyUsed)
            {
                return Json(new
                {
                    valid = false,
                    message = "You have already used this discount code"
                });
            }

            return Json(new
            {
                valid = true,
                message = "Discount code applied successfully",
                discountName = discount.DiscountName,
                discountType = discount.DiscountType.ToString(),
                discountValue = discount.DiscountValue
            });
        }
    }
}