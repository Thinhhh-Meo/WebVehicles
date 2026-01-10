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
    public class FeedbackController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FeedbackController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Feedback/Create
        public IActionResult Create(int? productId)
        {
            if (productId == null)
            {
                return NotFound();
            }

            var viewModel = new Feedback
            {
                ProductId = productId.Value
            };

            return View(viewModel);
        }

        // POST: Feedback/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Feedback feedback)
        {
            ModelState.Remove("UserId");
            ModelState.Remove("User");
            ModelState.Remove("Product");
            ModelState.Remove("FeedbackId");
            ModelState.Remove("CreatedDate");


            // Log tất cả ModelState errors
            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState Errors:");
                foreach (var key in ModelState.Keys)
                {
                    var errors = ModelState[key].Errors;
                    if (errors.Count > 0)
                    {
                        foreach (var error in errors)
                        {
                            Console.WriteLine($"- {key}: {error.ErrorMessage}");
                        }
                    }
                }
            }

            if (ModelState.IsValid)
            {
                feedback.UserId = _userManager.GetUserId(User);
                feedback.CreatedDate = DateTime.Now;

                _context.Feedbacks.Add(feedback);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Thank you for your feedback!";
                return RedirectToAction("Details", "Products", new { id = feedback.ProductId });
            }

            return View(feedback);
        }

        // GET: Feedback/MyFeedbacks
        public async Task<IActionResult> MyFeedbacks()
        {
            var userId = _userManager.GetUserId(User);
            var feedbacks = await _context.Feedbacks
                .Include(f => f.Product)
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.CreatedDate)
                .ToListAsync();

            return View(feedbacks);
        }

        // POST: Feedback/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);
            var feedback = await _context.Feedbacks
                .FirstOrDefaultAsync(f => f.FeedbackId == id && f.UserId == userId);

            if (feedback != null)
            {
                _context.Feedbacks.Remove(feedback);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Feedback deleted successfully";
            }

            return RedirectToAction(nameof(MyFeedbacks));
        }
    }
}