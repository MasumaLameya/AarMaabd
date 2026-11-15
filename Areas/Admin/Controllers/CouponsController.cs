using AarMaabd.Data;
using AarMaabd.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AarMaabd.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CouponsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CouponsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var coupons = await _context.Coupons
                .Include(c => c.Usages)
                .OrderByDescending(c => c.StartDateUtc)
                .ToListAsync();

            return View(coupons);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new Coupon { StartDateUtc = DateTime.UtcNow, EndDateUtc = DateTime.UtcNow.AddMonths(2) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Coupon coupon)
        {
            if (string.IsNullOrWhiteSpace(coupon.Code))
            {
                ModelState.AddModelError(nameof(coupon.Code), "Coupon Code is required.");
            }

            if (coupon.DiscountValue < 0)
            {
                ModelState.AddModelError(nameof(coupon.DiscountValue), "Discount value cannot be negative.");
            }

            if (coupon.DiscountType == DiscountType.Percentage && coupon.DiscountValue > 100)
            {
                ModelState.AddModelError(nameof(coupon.DiscountValue), "Percentage discount cannot exceed 100%.");
            }

            if (coupon.MaxDiscountCap.HasValue && coupon.MaxDiscountCap.Value < 0)
            {
                ModelState.AddModelError(nameof(coupon.MaxDiscountCap), "Maximum discount cap cannot be negative.");
            }

            if (coupon.MinSpendAmount < 0)
            {
                ModelState.AddModelError(nameof(coupon.MinSpendAmount), "Minimum spend amount cannot be negative.");
            }

            if (coupon.UsageLimitTotal.HasValue && coupon.UsageLimitTotal.Value < 0)
            {
                ModelState.AddModelError(nameof(coupon.UsageLimitTotal), "Total usage limit cannot be negative.");
            }

            if (coupon.UsageLimitPerCustomer < 0)
            {
                ModelState.AddModelError(nameof(coupon.UsageLimitPerCustomer), "Per customer usage limit cannot be negative.");
            }

            coupon.Code = coupon.Code?.Trim().ToUpperInvariant() ?? string.Empty;

            if (await _context.Coupons.AnyAsync(c => c.Code == coupon.Code))
            {
                ModelState.AddModelError(nameof(coupon.Code), "A coupon with this code already exists.");
            }

            if (ModelState.IsValid)
            {
                await _context.Coupons.AddAsync(coupon);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Coupon '{coupon.Code}' created successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(coupon);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var coupon = await _context.Coupons.FindAsync(id);
            if (coupon == null) return NotFound();

            coupon.IsActive = !coupon.IsActive;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Coupon '{coupon.Code}' is now {(coupon.IsActive ? "Active" : "Disabled")}.";
            return RedirectToAction(nameof(Index));
        }
    }
}
