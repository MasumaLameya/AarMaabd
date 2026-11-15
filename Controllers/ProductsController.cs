using AarMaabd.Data;
using AarMaabd.Models;
using AarMaabd.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AarMaabd.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Route("product/{slug}")]
        public async Task<IActionResult> Details(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug)) return NotFound();

            var product = await _context.Products
                .Include(p => p.Images.OrderBy(i => i.DisplayOrder))
                .Include(p => p.Variants.Where(v => v.IsActive))
                .Include(p => p.Category)
                .Include(p => p.SubCategory)
                .Include(p => p.Reviews.Where(r => r.IsApproved).OrderByDescending(r => r.CreatedAtUtc))
                .FirstOrDefaultAsync(p => p.Slug == slug && p.IsActive);

            if (product == null) return NotFound();

            var relatedProducts = await _context.Products
                .Include(p => p.Images)
                .Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id && p.IsActive)
                .Take(4)
                .ToListAsync();

            var reviews = product.Reviews.ToList();
            var avgRating = reviews.Any() ? reviews.Average(r => r.Rating) : 5.0;

            var model = new ProductDetailsViewModel
            {
                Product = product,
                RelatedProducts = relatedProducts,
                Reviews = reviews,
                AverageRating = Math.Round(avgRating, 1),
                TotalReviewsCount = reviews.Count
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(int productId, string customerName, int rating, string comment)
        {
            if (string.IsNullOrWhiteSpace(customerName) || string.IsNullOrWhiteSpace(comment) || rating < 1 || rating > 5)
            {
                TempData["ErrorMessage"] = "Please provide your name, valid rating and review comment.";
                return RedirectToAction(nameof(Details), new { slug = (await _context.Products.FindAsync(productId))?.Slug });
            }

            var product = await _context.Products.FindAsync(productId);
            if (product == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var review = new ProductReview
            {
                ProductId = productId,
                UserId = userId,
                CustomerName = customerName.Trim(),
                Rating = rating,
                Comment = comment.Trim(),
                IsApproved = true,
                CreatedAtUtc = DateTime.UtcNow
            };

            await _context.ProductReviews.AddAsync(review);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Thank you! Your review has been added.";
            return RedirectToAction(nameof(Details), new { slug = product.Slug });
        }
    }
}
