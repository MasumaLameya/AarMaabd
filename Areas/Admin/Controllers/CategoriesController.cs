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

            if (ModelState.IsValid)
            {
                if (string.IsNullOrWhiteSpace(category.Slug))
                {
                    category.Slug = category.Name.ToLowerInvariant().Replace(" ", "-").Replace("&", "and");
                }

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

                    category.ImageUrl = $"/images/categories/{fileName}";
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Category '{category.Name}' updated!";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }
    }
}
