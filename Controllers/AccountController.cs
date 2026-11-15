using AarMaabd.Data;
using AarMaabd.Models;
using AarMaabd.Services.Interfaces;
using AarMaabd.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AarMaabd.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;
        private readonly ApplicationDbContext _context;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ICartService cartService,
            IOrderService orderService,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _cartService = cartService;
            _orderService = orderService;
            _context = context;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Dashboard");
            }
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // Allow login by either Email or Phone Number
            var user = await _userManager.FindByEmailAsync(model.LoginIdentifier) 
                ?? await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == model.LoginIdentifier);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "No account found with this email or phone number.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName!, model.Password, model.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                user.LastLoginAtUtc = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                // Migrate guest cart to user
                await _cartService.MigrateGuestCartToUserAsync(user.Id);

                // If user is Admin, redirect to Admin dashboard
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                }

                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }

                return RedirectToAction("Dashboard");
            }

            ModelState.AddModelError(string.Empty, "Invalid login credentials. Please verify your password.");
            return View(model);
        }

        [HttpGet]
        public IActionResult Register(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Dashboard");
            }
            return View(new RegisterViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // Check if phone or email already in use
            if (await _userManager.Users.AnyAsync(u => u.PhoneNumber == model.PhoneNumber))
            {
                ModelState.AddModelError(nameof(model.PhoneNumber), "An account is already registered with this mobile number.");
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email.Trim().ToLowerInvariant(),
                Email = model.Email.Trim().ToLowerInvariant(),
                FullName = model.FullName.Trim(),
                PhoneNumber = model.PhoneNumber.Trim(),
                Division = model.Division,
                District = model.District,
                UpazilaThana = model.UpazilaThana,
                StreetAddress = model.StreetAddress,
                CreatedAtUtc = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Customer");
                await _signInManager.SignInAsync(user, isPersistent: true);

                // Migrate guest cart to user
                await _cartService.MigrateGuestCartToUserAsync(user.Id);

                TempData["SuccessMessage"] = "Welcome to AarMaabd! Your account has been created.";
                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }
                return RedirectToAction("Dashboard");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData["SuccessMessage"] = "You have been logged out safely.";
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);
            if (user == null) return NotFound();

            var orders = await _orderService.GetCustomerOrdersAsync(userId!);
            var customOrders = await _context.CustomOrderRequests
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedAtUtc)
                .ToListAsync();

            var wishlist = await _context.WishlistItems
                .Include(w => w.Product)
                    .ThenInclude(p => p!.Images)
                .Where(w => w.UserId == userId)
                .ToListAsync();

            var model = new CustomerDashboardViewModel
            {
                User = user,
                RecentOrders = orders,
                CustomOrders = customOrders,
                Wishlist = wishlist,
                TotalOrdersCount = orders.Count,
                TotalSpent = orders.Where(o => o.Status != OrderStatus.Cancelled).Sum(o => o.TotalAmount)
            };

            return View(model);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> OrderDetails(string orderNumber)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var order = await _orderService.GetOrderByNumberAsync(orderNumber);

            if (order == null || (order.UserId != userId && !User.IsInRole("Admin")))
            {
                return NotFound();
            }

            return View(order);
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
