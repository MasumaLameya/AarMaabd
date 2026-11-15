using AarMaabd.Data;
using AarMaabd.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AarMaabd.Controllers
{
    [Authorize]
    public class CustomOrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CustomOrdersController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new CustomOrderRequest();
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    model.CustomerName = user.FullName;
                    model.CustomerEmail = user.Email;
                    model.CustomerPhone = user.PhoneNumber ?? string.Empty;
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomOrderRequest model, IFormFile? referenceImage)
        {
            if (string.IsNullOrWhiteSpace(model.CustomerName) || 
                string.IsNullOrWhiteSpace(model.CustomerPhone) || 
                string.IsNullOrWhiteSpace(model.Description))
            {
                ModelState.AddModelError(string.Empty, "Please fill in your name, contact phone, and project specifications.");
                return View(model);
            }

            if (model.EstimatedBudget.HasValue && model.EstimatedBudget.Value < 0)
            {
                ModelState.AddModelError(nameof(model.EstimatedBudget), "Estimated budget cannot be negative.");
                return View(model);
            }

            if (referenceImage != null && referenceImage.Length > 0)
            {
                var ext = Path.GetExtension(referenceImage.FileName).ToLowerInvariant();
                var allowedExts = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                if (!allowedExts.Contains(ext))
                {
                    ModelState.AddModelError(string.Empty, "Only image files (.jpg, .jpeg, .png, .webp) are allowed.");
                    return View(model);
                }

                if (referenceImage.Length > 10 * 1024 * 1024)
                {
                    ModelState.AddModelError(string.Empty, "Image file size cannot exceed 10 MB.");
                    return View(model);
                }

                var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "custom_orders");
                if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);

                var fileName = $"{Guid.NewGuid():N}{ext}";
                var filePath = Path.Combine(uploadsDir, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await referenceImage.CopyToAsync(stream);
                }
                model.ReferenceImageUrl = $"/images/custom_orders/{fileName}";
            }

            model.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            model.Status = CustomOrderStatus.Requested;
            model.CreatedAtUtc = DateTime.UtcNow;

            await _context.CustomOrderRequests.AddAsync(model);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your custom handmade request has been submitted! Our team will review and contact you with a quotation.";
            return RedirectToAction("Success", new { id = model.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Success(int id)
        {
            var request = await _context.CustomOrderRequests.FindAsync(id);
            if (request == null) return RedirectToAction("Create");

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (request.UserId != null && request.UserId != currentUserId && !User.IsInRole("Admin"))
            {
                return NotFound();
            }

            return View(request);
        }
    }
}
