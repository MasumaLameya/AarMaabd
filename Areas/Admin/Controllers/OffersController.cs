using AarMaabd.Data;
using AarMaabd.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AarMaabd.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OffersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OffersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var offers = await _context.Offers
                .Include(o => o.TargetCategory)
                .OrderBy(o => o.DisplayOrder)
                .ToListAsync();

            return View(offers);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = new SelectList(await _context.Categories.Where(c => c.IsActive).ToListAsync(), "Id", "Name");
            return View(new Offer { StartDateUtc = DateTime.UtcNow, EndDateUtc = DateTime.UtcNow.AddMonths(1) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Offer offer)
        {
            if (string.IsNullOrWhiteSpace(offer.Title))
            {
                ModelState.AddModelError(nameof(offer.Title), "Offer Title is required.");
            }

            if (offer.DiscountValue < 0)
            {
                ModelState.AddModelError(nameof(offer.DiscountValue), "Discount value cannot be negative.");
            }

            if (offer.DiscountType == DiscountType.Percentage && offer.DiscountValue > 100)
            {
                ModelState.AddModelError(nameof(offer.DiscountValue), "Discount percentage cannot exceed 100%.");
            }

            if (offer.MinOrderAmount < 0)
            {
                ModelState.AddModelError(nameof(offer.MinOrderAmount), "Minimum order amount cannot be negative.");
            }

            if (ModelState.IsValid)
            {
                await _context.Offers.AddAsync(offer);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Offer '{offer.Title}' published successfully!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = new SelectList(await _context.Categories.Where(c => c.IsActive).ToListAsync(), "Id", "Name", offer.TargetCategoryId);
            return View(offer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var offer = await _context.Offers.FindAsync(id);
            if (offer == null) return NotFound();

            offer.IsActive = !offer.IsActive;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Offer '{offer.Title}' is now {(offer.IsActive ? "Active" : "Disabled")}.";
            return RedirectToAction(nameof(Index));
        }
    }
}
