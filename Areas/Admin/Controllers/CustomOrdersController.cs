using AarMaabd.Data;
using AarMaabd.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AarMaabd.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CustomOrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomOrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var requests = await _context.CustomOrderRequests
                .OrderByDescending(c => c.CreatedAtUtc)
                .ToListAsync();

            return View(requests);
        }

        public async Task<IActionResult> Details(int id)
        {
            var request = await _context.CustomOrderRequests.FindAsync(id);
            if (request == null) return NotFound();
            return View(request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuote(int id, CustomOrderStatus status, decimal? quotedPrice, string? adminNotes)
        {
            var request = await _context.CustomOrderRequests.FindAsync(id);
            if (request == null) return NotFound();

            if (quotedPrice.HasValue && quotedPrice.Value < 0)
            {
                TempData["ErrorMessage"] = "Quoted price cannot be negative.";
                return RedirectToAction(nameof(Details), new { id });
            }

            request.Status = status;
            request.QuotedPrice = quotedPrice;
            request.AdminNotes = adminNotes;
            request.UpdatedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Custom order request #{id} updated successfully!";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
