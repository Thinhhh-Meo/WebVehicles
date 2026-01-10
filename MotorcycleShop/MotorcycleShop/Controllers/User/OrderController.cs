    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using MotorcycleShop.Data;
    using MotorcycleShop.Models;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Threading.Tasks;

    namespace MotorcycleShop.Controllers
    {
        [Authorize]
        public class OrderController : Controller
        {
            private readonly ApplicationDbContext _context;
            private readonly UserManager<ApplicationUser> _userManager;

            public OrderController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
            {
                _context = context;
                _userManager = userManager;
            }

            // ========== CHECKOUT ==========

            // GET: Order/Checkout
            public async Task<IActionResult> Checkout()
            {
                var userId = _userManager.GetUserId(User);
                var user = await _userManager.FindByIdAsync(userId);

                // Get user's cart with items
                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.Product)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null || !cart.CartItems.Any())
                {
                    TempData["ErrorMessage"] = "Your cart is empty";
                    return RedirectToAction("Index", "Cart");
                }

                decimal totalPrice = cart.CartItems.Sum(ci => ci.Quantity * ci.Product.Price);

                var viewModel = new CheckoutViewModel
                {
                    FullName = user.FullName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Address = user.Address,
                    CartItems = cart.CartItems.ToList(),
                    TotalPrice = totalPrice,
                    FinalPrice = totalPrice
                };

                return View(viewModel);
            }

        // AJAX: Validate discount code
        [HttpGet]
        public async Task<JsonResult> ValidateDiscount(string code)
        {
            try
            {
                Console.WriteLine($"ValidateDiscount called with code: {code}");

                if (string.IsNullOrEmpty(code))
                {
                    return Json(new { success = false, message = "Please enter discount code" });
                }

                var userId = _userManager.GetUserId(User);

                // Get cart total
                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.Product)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null || !cart.CartItems.Any())
                {
                    return Json(new { success = false, message = "Cart is empty" });
                }

                decimal totalPrice = cart.CartItems.Sum(ci => ci.Quantity * ci.Product.Price);

                // Find active discount
                var discount = await _context.Discounts
                    .FirstOrDefaultAsync(d => d.Code.ToUpper() == code.Trim().ToUpper() &&
                                              d.IsActive &&
                                              d.DateStart <= DateTime.Now &&
                                              d.DateEnd >= DateTime.Now &&
                                              d.UsedCount < (d.UsageLimit ?? int.MaxValue));

                if (discount == null)
                {
                    return Json(new { success = false, message = "Invalid or expired discount code" });
                }

                // BỎ KIỂM TRA MIN ORDER AMOUNT
                // if (discount.MinOrderAmount.HasValue && totalPrice < discount.MinOrderAmount.Value)
                // {
                //     return Json(new
                //     {
                //         success = false,
                //         message = $"Minimum order amount: {discount.MinOrderAmount.Value.ToString("N0")} đ"
                //     });
                // }

                // Check if user already used this discount
                bool alreadyUsed = await _context.UserDiscounts
                    .AnyAsync(ud => ud.UserId == userId && ud.DiscountId == discount.DiscountId);

                if (alreadyUsed)
                {
                    return Json(new { success = false, message = "You have already used this discount code" });
                }

                // Calculate discount amount
                decimal discountAmount = 0;
                if (discount.DiscountType == DiscountType.Percentage)
                {
                    discountAmount = totalPrice * (decimal)(discount.DiscountValue / 100);
                }
                else if (discount.DiscountType == DiscountType.FixedAmount)
                {
                    discountAmount = (decimal)discount.DiscountValue;
                    if (discountAmount > totalPrice)
                    {
                        discountAmount = totalPrice;
                    }
                }

                decimal finalPrice = totalPrice - discountAmount;

                return Json(new
                {
                    success = true,
                    message = $"Discount '{discount.DiscountName}' applied successfully!",
                    discountName = discount.DiscountName,
                    discountType = discount.DiscountType.ToString(),
                    discountValue = discount.DiscountValue,
                    discountAmount = discountAmount,
                    totalPrice = totalPrice,
                    finalPrice = finalPrice,
                    code = discount.Code
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in ValidateDiscount: {ex.Message}");
                return Json(new { success = false, message = "Error processing discount code" });
            }
        }

        // POST: Order/Checkout - SỬA HOÀN TOÀN VỚI TRANSACTION
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            Console.WriteLine("=== CHECKOUT START ===");
            Console.WriteLine($"Model DiscountCode: {model.DiscountCode}");

            var userId = _userManager.GetUserId(User);

            // Sử dụng transaction để đảm bảo tính toàn vẹn dữ liệu
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Lấy cart từ database
                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.Product)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null || !cart.CartItems.Any())
                {
                    TempData["ErrorMessage"] = "Your cart is empty";
                    return RedirectToAction("Index", "Cart");
                }

                // Validate stock quantities
                foreach (var cartItem in cart.CartItems)
                {
                    if (cartItem.Product.Quantity < cartItem.Quantity)
                    {
                        TempData["ErrorMessage"] = $"{cartItem.Product.Name} is out of stock";
                        return RedirectToAction("Index", "Cart");
                    }
                }

                //  3.Tính totalPrice từ database
                decimal totalPrice = cart.CartItems.Sum(ci => ci.Quantity * ci.Product.Price);
                Console.WriteLine($"Database TotalPrice: {totalPrice}");

                decimal discountAmount = 0;
                Discount? discount = null;

                // 4. Xử lý discount nếu có code
                 

                if (!string.IsNullOrEmpty(model.DiscountCode))
                {
                    Console.WriteLine($"Processing discount code: {model.DiscountCode}");

                    // Tìm discount hợp lệ - TRONG TRANSACTION
                    discount = await _context.Discounts
                        .FirstOrDefaultAsync(d => d.Code.ToUpper() == model.DiscountCode.Trim().ToUpper() &&
                                                  d.IsActive &&
                                                  d.DateStart <= DateTime.Now &&
                                                  d.DateEnd >= DateTime.Now &&
                                                  d.UsedCount < (d.UsageLimit ?? int.MaxValue));

                    if (discount == null)
                    {
                        ModelState.AddModelError("DiscountCode", "Invalid discount code");

                        // Reset giá trước khi render lại view
                        model.DiscountAmount = 0;
                        model.FinalPrice = totalPrice;

                        return SetupCheckoutView(model, cart, totalPrice);
                    }

                    Console.WriteLine($"Found discount: {discount.DiscountName}, Current UsedCount: {discount.UsedCount}");

                    // Kiểm tra alreadyUsed - TRONG TRANSACTION
                    bool alreadyUsed = await _context.UserDiscounts
                        .AnyAsync(ud => ud.UserId == userId && ud.DiscountId == discount.DiscountId);

                    if (alreadyUsed)
                    {
                        Console.WriteLine($"User {userId} already used discount {discount.DiscountId}");
                        ModelState.AddModelError("DiscountCode", "You have already used this discount code");

                        // Reset giá trước khi render lại view
                        model.DiscountAmount = 0;
                        model.FinalPrice = totalPrice;

                        return SetupCheckoutView(model, cart, totalPrice);
                    }

                    // Tính discount amount từ database
                    if (discount.DiscountType == DiscountType.Percentage)
                    {
                        discountAmount = totalPrice * (decimal)(discount.DiscountValue / 100);
                        Console.WriteLine($"Percentage discount: {discount.DiscountValue}% = {discountAmount}");
                    }
                    else if (discount.DiscountType == DiscountType.FixedAmount)
                    {
                        discountAmount = (decimal)discount.DiscountValue;
                        if (discountAmount > totalPrice)
                        {
                            discountAmount = totalPrice;
                        }
                        Console.WriteLine($"Fixed amount discount: {discountAmount}");
                    }

                    // Cập nhật vào model
                    model.DiscountAmount = discountAmount;
                }

                // Tính final price (dùng discountAmount đã tính)
                model.FinalPrice = totalPrice - discountAmount;
                Console.WriteLine($"FINAL PRICE: {totalPrice} - {discountAmount} = {model.FinalPrice}");


                // Tạo order
                var order = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.Now,
                    Status = OrderStatus.Pending,
                    PaymentMethod = model.PaymentMethod,
                    TotalPrice = totalPrice,
                    DiscountAmount = discountAmount,
                    FinalPrice = model.FinalPrice,
                    ShippingAddress = model.Address,
                    PhoneNumber = model.PhoneNumber,
                    Note = model.Note,
                    DiscountId = discount?.DiscountId
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // 7️⃣ GHI NHẬN DISCOUNT — SAU KHI ORDER OK
                if (discount != null)
                {
                    _context.UserDiscounts.Add(new UserDiscount
                    {
                        UserId = userId,
                        DiscountId = discount.DiscountId,
                        UsedDate = DateTime.Now
                    });

                    discount.UsedCount++;
                    _context.Discounts.Update(discount);

                    await _context.SaveChangesAsync();
                }

                Console.WriteLine($"Order #{order.OrderId} created with FinalPrice: {order.FinalPrice}");

                // Tạo order details và update stock
                foreach (var cartItem in cart.CartItems)
                {
                    var orderDetail = new OrderDetail
                    {
                        OrderId = order.OrderId,
                        ProductId = cartItem.ProductId,
                        Quantity = cartItem.Quantity,
                        UnitPrice = cartItem.Product.Price,
                    };
                    _context.OrderDetails.Add(orderDetail);

                    // Update product quantity
                    cartItem.Product.Quantity -= cartItem.Quantity;
                    _context.Products.Update(cartItem.Product);
                }

                // Clear cart
                _context.CartItems.RemoveRange(cart.CartItems);
                _context.Carts.Remove(cart);

                // Lưu tất cả thay đổi
                await _context.SaveChangesAsync();

                // **COMMIT TRANSACTION**
                await transaction.CommitAsync();
                Console.WriteLine("Transaction committed successfully");

                TempData["SuccessMessage"] = $"Order #{order.OrderId} placed successfully!";
                return RedirectToAction("Details", new { id = order.OrderId });
            }
            catch (Exception ex)
            {
                // **ROLLBACK TRANSACTION** nếu có lỗi
                await transaction.RollbackAsync();
                Console.WriteLine($"TRANSACTION ROLLED BACK: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");

                TempData["ErrorMessage"] = "Error processing order. Please try again.";
                return RedirectToAction("Checkout");
            }
        }

        // Helper method
        private IActionResult SetupCheckoutView(CheckoutViewModel model, Cart cart, decimal totalPrice)
        {
            model.CartItems = cart.CartItems.ToList();
            model.TotalPrice = totalPrice;

            model.FinalPrice = totalPrice - model.DiscountAmount;
            


            return View(model);
        }
        // ========== ORDER MANAGEMENT ==========

        // GET: Order/MyOrders
        public async Task<IActionResult> MyOrders()
            {
                var userId = _userManager.GetUserId(User);
                var orders = await _context.Orders
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Product)
                    .Include(o => o.Discount)
                    .Where(o => o.UserId == userId)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();

                return View(orders);
            }

            // GET: Order/Details/5
            public async Task<IActionResult> Details(int id)
            {
                var userId = _userManager.GetUserId(User);
                var order = await _context.Orders
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Product)
                    .Include(o => o.Discount)
                    .Include(o => o.User) // THÊM INCLUDE USER
                    .FirstOrDefaultAsync(o => o.OrderId == id && o.UserId == userId);

                if (order == null)
                {
                    return NotFound();
                }

                return View(order);
            }

            // POST: Order/Cancel/5
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Cancel(int id)
            {
                var userId = _userManager.GetUserId(User);
                var order = await _context.Orders
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Product)
                    .FirstOrDefaultAsync(o => o.OrderId == id && o.UserId == userId);

                if (order == null)
                {
                    return NotFound();
                }

                if (order.Status == OrderStatus.Pending)
                {
                    order.Status = OrderStatus.Cancelled;

                    // Restore product quantities
                    foreach (var detail in order.OrderDetails)
                    {
                        detail.Product.Quantity += detail.Quantity;
                        _context.Products.Update(detail.Product);
                    }

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Order #{order.OrderId} cancelled successfully";
                }
                else
                {
                    TempData["ErrorMessage"] = "Only pending orders can be cancelled";
                }

                return RedirectToAction(nameof(MyOrders));
            }
        }

        // ========== VIEW MODELS ==========

        public class CheckoutViewModel
        {
            [Required(ErrorMessage = "Full name is required")]
            [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
            [Display(Name = "Full Name")]
            public string FullName { get; set; }

            [Required(ErrorMessage = "Email is required")]
            [EmailAddress(ErrorMessage = "Invalid email address")]
            [Display(Name = "Email Address")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Phone number is required")]
            [Phone(ErrorMessage = "Invalid phone number")]
            [Display(Name = "Phone Number")]
            public string PhoneNumber { get; set; }

            [Required(ErrorMessage = "Address is required")]
            [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
            [Display(Name = "Shipping Address")]
            public string Address { get; set; }

            [StringLength(500, ErrorMessage = "Note cannot exceed 500 characters")]
            [Display(Name = "Order Note (Optional)")]
            public string? Note { get; set; }

            [Required(ErrorMessage = "Please select payment method")]
            [Display(Name = "Payment Method")]
            public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.COD;

            [StringLength(50, ErrorMessage = "Discount code cannot exceed 50 characters")]
            [Display(Name = "Discount Code")]
            public string? DiscountCode { get; set; }

            // Cart data
            public List<CartItem> CartItems { get; set; } = new List<CartItem>();

            // Prices
            public decimal TotalPrice { get; set; }
            public decimal DiscountAmount { get; set; }
            public decimal FinalPrice { get; set; }
        }
    }