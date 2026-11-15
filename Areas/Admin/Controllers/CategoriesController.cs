using AarMaabd.Data;
using AarMaabd.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AarMaabd.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                .Include(c => c.Products)
                .Include(c => c.SubCategories)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            return View(categories);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new Category());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category, IFormFile? categoryImage)
        {
            if (string.IsNullOrWhiteSpace(category.Name))
            {
                ModelState.AddModelError(nameof(category.Name), "Category Name is required.");
            }
            else if (string.IsNullOrWhiteSpace(category.Slug))
            {
                var cleanSlug = category.Name.ToLowerInvariant().Replace(" ", "-").Replace("&", "and");
                cleanSlug = System.Text.RegularExpressions.Regex.Replace(cleanSlug, @"[^a-z0-9\-]", "");
                var baseSlug = cleanSlug;
                int suffix = 1;
                while (await _context.Categories.AnyAsync(c => c.Slug == cleanSlug))
                {
                    cleanSlug = $"{baseSlug}-{suffix++}";
                }
                category.Slug = cleanSlug;
                ModelState.Remove(nameof(category.Slug));
            }

            if (ModelState.IsValid)
            {

                if (categoryImage != null && categoryImage.Length > 0)
                {
                    var ext = Path.GetExtension(categoryImage.FileName).ToLowerInvariant();
                    var allowedExts = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                    if (!allowedExts.Contains(ext))
                    {
                        ModelState.AddModelError(string.Empty, "Only image files (.jpg, .jpeg, .png, .webp) are allowed.");
                        return View(category);
                    }

                    if (categoryImage.Length > 10 * 1024 * 1024)
                    {
                        ModelState.AddModelError(string.Empty, "Image file size cannot exceed 10 MB.");
                        return View(category);
                    }

                    var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "categories");
                    if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);

                    var fileName = $"{Guid.NewGuid():N}{ext}";
                    var filePath = Path.Combine(uploadsDir, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await categoryImage.CopyToAsync(stream);
                    }

                    category.ImageUrl = $"/images/categories/{fileName}";
                }

                await _context.Categories.AddAsync(category);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Category '{category.Name}' created!";
                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category model, IFormFile? newImage)
        {
            if (id != model.Id) return NotFound();

            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError(nameof(model.Name), "Category Name is required.");
            }
            else if (string.IsNullOrWhiteSpace(model.Slug))
            {
                model.Slug = model.Name.ToLowerInvariant().Replace(" ", "-").Replace("&", "and");
                model.Slug = System.Text.RegularExpressions.Regex.Replace(model.Slug, @"[^a-z0-9\-]", "");
                ModelState.Remove(nameof(model.Slug));
            }

            if (ModelState.IsValid)
            {
                category.Name = model.Name;
                category.Slug = model.Slug;
                category.Description = model.Description;
                category.IconClass = model.IconClass;
                category.DisplayOrder = model.DisplayOrder;
                category.IsActive = model.IsActive;

                if (newImage != null && newImage.Length > 0)
                {
                    var ext = Path.GetExtension(newImage.FileName).ToLowerInvariant();
                    var allowedExts = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                    if (!allowedExts.Contains(ext))
                    {
                        ModelState.AddModelError(string.Empty, "Only image files (.jpg, .jpeg, .png, .webp) are allowed.");
                        return View(model);
                    }

                    if (newImage.Length > 10 * 1024 * 1024)
                    {
                        ModelState.AddModelError(string.Empty, "Image file size cannot exceed 10 MB.");
                        return View(model);
                    }

                    var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "categories");
                    if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);

                    var fileName = $"{Guid.NewGuid():N}{ext}";
                    var filePath = Path.Combine(uploadsDir, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await newImage.CopyToAsync(stream);
                    }

                    // Delete old local image if any
                    if (!string.IsNullOrEmpty(category.ImageUrl) && category.ImageUrl.StartsWith("/images/categories/"))
                    {
                        var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", category.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldPath))
                        {
                            try { System.IO.File.Delete(oldPath); } catch { }
                        }
                    }

                    category.ImageUrl = $"/images/categories/{fileName}";
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Category '{category.Name}' updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .Include(c => c.SubCategories)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                TempData["ErrorMessage"] = "Category not found.";
                return RedirectToAction(nameof(Index));
            }

            if (category.Products.Any())
            {
                TempData["ErrorMessage"] = $"Cannot delete category '{category.Name}' because it currently contains {category.Products.Count} product(s). Please move or delete those products first, or disable the category.";
                return RedirectToAction(nameof(Index));
            }

            // Remove subcategories if any
            if (category.SubCategories.Any())
            {
                _context.SubCategories.RemoveRange(category.SubCategories);
            }

            // Remove image file if stored locally
            if (!string.IsNullOrEmpty(category.ImageUrl) && category.ImageUrl.StartsWith("/images/categories/"))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", category.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    try { System.IO.File.Delete(filePath); } catch { }
                }
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Category '{category.Name}' was deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            category.IsActive = !category.IsActive;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Category '{category.Name}' is now {(category.IsActive ? "Active" : "Disabled")}.";
            return RedirectToAction(nameof(Index));
        }
    }
}
