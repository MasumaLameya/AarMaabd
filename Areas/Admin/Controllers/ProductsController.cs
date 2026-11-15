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
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? search, int? categoryId)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(term) || p.SKU.ToLower().Contains(term));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            var products = await query.OrderByDescending(p => p.CreatedAtUtc).ToListAsync();

            ViewBag.Categories = new SelectList(await _context.Categories.Where(c => c.IsActive).ToListAsync(), "Id", "Name", categoryId);
            ViewBag.Search = search;

            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var categories = await _context.Categories.Where(c => c.IsActive).OrderBy(c => c.DisplayOrder).ToListAsync();
            ViewBag.CategoriesList = categories;
            ViewBag.CategoryId = new SelectList(categories, "Id", "Name");
            var defaultCatId = categories.FirstOrDefault()?.Id ?? 0;
            var initialSku = await GenerateUniqueSkuAsync(defaultCatId, null);
            return View(new Product { SKU = initialSku });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile? primaryImage)
        {
            // Auto-generate or validate SKU
            if (string.IsNullOrWhiteSpace(product.SKU))
            {
                product.SKU = await GenerateUniqueSkuAsync(product.CategoryId, product.Name);
                ModelState.Remove(nameof(product.SKU));
            }
            else
            {
                if (await _context.Products.AnyAsync(p => p.SKU == product.SKU.Trim()))
                {
                    product.SKU = await GenerateUniqueSkuAsync(product.CategoryId, product.Name);
                    ModelState.Remove(nameof(product.SKU));
                }
                else
                {
                    product.SKU = product.SKU.Trim().ToUpperInvariant();
                    ModelState.Remove(nameof(product.SKU));
                }
            }

            if (string.IsNullOrWhiteSpace(product.Name))
            {
                ModelState.AddModelError(nameof(product.Name), "Product Name is required.");
            }
            else if (string.IsNullOrWhiteSpace(product.Slug))
            {
                // Auto generate slug if empty
                var cleanSlug = product.Name.ToLowerInvariant().Replace(" ", "-").Replace("&", "and");
                cleanSlug = System.Text.RegularExpressions.Regex.Replace(cleanSlug, @"[^a-z0-9\-]", "");
                var baseSlug = cleanSlug;
                int suffix = 1;
                while (await _context.Products.AnyAsync(p => p.Slug == cleanSlug))
                {
                    cleanSlug = $"{baseSlug}-{suffix++}";
                }
                product.Slug = cleanSlug;
                ModelState.Remove(nameof(product.Slug));
            }

            if (product.Price < 0)
            {
                ModelState.AddModelError(nameof(product.Price), "Regular price cannot be negative.");
            }

            if (product.SalePrice.HasValue && product.SalePrice.Value < 0)
            {
                ModelState.AddModelError(nameof(product.SalePrice), "Sale price cannot be negative.");
            }

            if (product.StockQuantity < 0)
            {
                ModelState.AddModelError(nameof(product.StockQuantity), "Stock quantity cannot be negative.");
            }

            if (product.LowStockThreshold < 0)
            {
                ModelState.AddModelError(nameof(product.LowStockThreshold), "Low stock threshold cannot be negative.");
            }

            if (product.PreparationLeadDays < 0)
            {
                ModelState.AddModelError(nameof(product.PreparationLeadDays), "Preparation lead days cannot be negative.");
            }

            if (ModelState.IsValid)
            {

                if (primaryImage != null && primaryImage.Length > 0)
                {
                    var ext = Path.GetExtension(primaryImage.FileName).ToLowerInvariant();
                    var allowedExts = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                    if (!allowedExts.Contains(ext))
                    {
                        ModelState.AddModelError(string.Empty, "Only image files (.jpg, .jpeg, .png, .webp) are allowed.");
                        ViewBag.Categories = new SelectList(await _context.Categories.Where(c => c.IsActive).ToListAsync(), "Id", "Name", product.CategoryId);
                        return View(product);
                    }

                    if (primaryImage.Length > 10 * 1024 * 1024)
                    {
                        ModelState.AddModelError(string.Empty, "Image file size cannot exceed 10 MB.");
                        ViewBag.Categories = new SelectList(await _context.Categories.Where(c => c.IsActive).ToListAsync(), "Id", "Name", product.CategoryId);
                        return View(product);
                    }

                    var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products");
                    if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);

                    var fileName = $"{Guid.NewGuid():N}{ext}";
                    var filePath = Path.Combine(uploadsDir, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await primaryImage.CopyToAsync(stream);
                    }

                    product.Images.Add(new ProductImage
                    {
                        ImageUrl = $"/images/products/{fileName}",
                        AltText = product.Name,
                        IsPrimary = true,
                        DisplayOrder = 1
                    });
                }
                else
                {
                    // Default image fallback
                    product.Images.Add(new ProductImage
                    {
                        ImageUrl = "/images/products/crochet-bag.jpg",
                        AltText = product.Name,
                        IsPrimary = true,
                        DisplayOrder = 1
                    });
                }

                product.CreatedAtUtc = DateTime.UtcNow;
                await SanitizeProductAttributesByCategoryAsync(product);
                await _context.Products.AddAsync(product);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Product '{product.Name}' created successfully!";
                return RedirectToAction(nameof(Index));
            }

            var allCats = await _context.Categories.Where(c => c.IsActive).OrderBy(c => c.DisplayOrder).ToListAsync();
            ViewBag.CategoriesList = allCats;
            ViewBag.CategoryId = new SelectList(allCats, "Id", "Name", product.CategoryId);
            return View(product);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            var categories = await _context.Categories.Where(c => c.IsActive).OrderBy(c => c.DisplayOrder).ToListAsync();
            ViewBag.CategoriesList = categories;
            ViewBag.CategoryId = new SelectList(categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product model, IFormFile? newImage)
        {
            if (id != model.Id) return NotFound();

            var product = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            // Auto-generate or validate SKU on Edit
            if (string.IsNullOrWhiteSpace(model.SKU))
            {
                model.SKU = await GenerateUniqueSkuAsync(model.CategoryId, model.Name);
                ModelState.Remove(nameof(model.SKU));
            }
            else
            {
                if (await _context.Products.AnyAsync(p => p.Id != id && p.SKU == model.SKU.Trim()))
                {
                    model.SKU = await GenerateUniqueSkuAsync(model.CategoryId, model.Name);
                    ModelState.Remove(nameof(model.SKU));
                }
                else
                {
                    model.SKU = model.SKU.Trim().ToUpperInvariant();
                    ModelState.Remove(nameof(model.SKU));
                }
            }

            if (string.IsNullOrWhiteSpace(model.Slug) && !string.IsNullOrWhiteSpace(model.Name))
            {
                var cleanSlug = model.Name.ToLowerInvariant().Replace(" ", "-").Replace("&", "and");
                cleanSlug = System.Text.RegularExpressions.Regex.Replace(cleanSlug, @"[^a-z0-9\-]", "");
                var baseSlug = cleanSlug;
                int suffix = 1;
                while (await _context.Products.AnyAsync(p => p.Id != id && p.Slug == cleanSlug))
                {
                    cleanSlug = $"{baseSlug}-{suffix++}";
                }
                model.Slug = cleanSlug;
                ModelState.Remove(nameof(model.Slug));
            }

            if (model.Price < 0)
            {
                ModelState.AddModelError(nameof(model.Price), "Regular price cannot be negative.");
            }

            if (model.SalePrice.HasValue && model.SalePrice.Value < 0)
            {
                ModelState.AddModelError(nameof(model.SalePrice), "Sale price cannot be negative.");
            }

            if (model.StockQuantity < 0)
            {
                ModelState.AddModelError(nameof(model.StockQuantity), "Stock quantity cannot be negative.");
            }

            if (model.LowStockThreshold < 0)
            {
                ModelState.AddModelError(nameof(model.LowStockThreshold), "Low stock threshold cannot be negative.");
            }

            if (model.PreparationLeadDays < 0)
            {
                ModelState.AddModelError(nameof(model.PreparationLeadDays), "Preparation lead days cannot be negative.");
            }

            if (ModelState.IsValid)
            {
                product.Name = model.Name;
                product.Slug = model.Slug;
                product.SKU = model.SKU;
                product.CategoryId = model.CategoryId;
                product.Price = model.Price;
                product.SalePrice = model.SalePrice;
                product.StockQuantity = model.StockQuantity;
                product.LowStockThreshold = model.LowStockThreshold;
                product.ShortDescription = model.ShortDescription;
                product.Description = model.Description;
                product.ProductType = model.ProductType;
                product.IsActive = model.IsActive;
                product.IsFeatured = model.IsFeatured;
                product.IsNewArrival = model.IsNewArrival;
                product.IsBestSeller = model.IsBestSeller;
                product.AvailableSizes = model.AvailableSizes;
                product.AvailableColors = model.AvailableColors;
                product.AvailableFlavors = model.AvailableFlavors;
                product.Ingredients = model.Ingredients;
                product.CareInstructions = model.CareInstructions;
                product.PreparationLeadDays = model.PreparationLeadDays;
                product.UpdatedAtUtc = DateTime.UtcNow;

                if (newImage != null && newImage.Length > 0)
                {
                    var ext = Path.GetExtension(newImage.FileName).ToLowerInvariant();
                    var allowedExts = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                    if (!allowedExts.Contains(ext))
                    {
                        ModelState.AddModelError(string.Empty, "Only image files (.jpg, .jpeg, .png, .webp) are allowed.");
                        ViewBag.Categories = new SelectList(await _context.Categories.Where(c => c.IsActive).ToListAsync(), "Id", "Name", model.CategoryId);
                        return View(model);
                    }

                    if (newImage.Length > 10 * 1024 * 1024)
                    {
                        ModelState.AddModelError(string.Empty, "Image file size cannot exceed 10 MB.");
                        ViewBag.Categories = new SelectList(await _context.Categories.Where(c => c.IsActive).ToListAsync(), "Id", "Name", model.CategoryId);
                        return View(model);
                    }

                    var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products");
                    if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);

                    var fileName = $"{Guid.NewGuid():N}{ext}";
                    var filePath = Path.Combine(uploadsDir, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await newImage.CopyToAsync(stream);
                    }

                    // Set existing to non-primary
                    foreach (var img in product.Images) img.IsPrimary = false;

                    product.Images.Add(new ProductImage
                    {
                        ImageUrl = $"/images/products/{fileName}",
                        AltText = product.Name,
                        IsPrimary = true,
                        DisplayOrder = product.Images.Count + 1
                    });
                }

                await SanitizeProductAttributesByCategoryAsync(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Product '{product.Name}' updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            var allCategories = await _context.Categories.Where(c => c.IsActive).OrderBy(c => c.DisplayOrder).ToListAsync();
            ViewBag.CategoriesList = allCategories;
            ViewBag.CategoryId = new SelectList(allCategories, "Id", "Name", model.CategoryId);
            return View(model);
        }

        private async Task SanitizeProductAttributesByCategoryAsync(Product product)
        {
            var category = await _context.Categories.FindAsync(product.CategoryId);
            if (category == null) return;

            var slug = category.Slug.ToLowerInvariant();

            bool isFood = slug == "cakes-desserts" || slug == "savory-food";
            bool isCandle = slug == "gypsum-candles";
            bool isBeauty = slug == "makeup-beauty";

            // Flavors: Only Food and Candles (as scents)
            if (!isFood && !isCandle)
            {
                product.AvailableFlavors = null;
            }

            // Ingredients: Only Food and Beauty
            if (!isFood && !isBeauty)
            {
                product.Ingredients = null;
            }

            // Colors: Everything except food items
            if (isFood)
            {
                product.AvailableColors = null;
            }
        }

        private async Task<string> GenerateUniqueSkuAsync(int categoryId, string? productName)
        {
            var category = await _context.Categories.FindAsync(categoryId);
            var slug = category?.Slug?.ToLowerInvariant() ?? "";

            string prefix = "AM";
            if (slug.Contains("crochet")) prefix = "CR";
            else if (slug.Contains("cake") || slug.Contains("dessert")) prefix = "CK";
            else if (slug.Contains("food") || slug.Contains("savory")) prefix = "FD";
            else if (slug.Contains("candle") || slug.Contains("gypsum")) prefix = "CN";
            else if (slug.Contains("resin")) prefix = "RS";
            else if (slug.Contains("jewel")) prefix = "JW";
            else if (slug.Contains("beauty") || slug.Contains("makeup")) prefix = "MB";
            else if (slug.Contains("clothes") || slug.Contains("wear")) prefix = "CL";
            else if (slug.Contains("bag") || slug.Contains("purse")) prefix = "BG";
            else if (slug.Contains("gift")) prefix = "GF";

            string middle = "";
            if (!string.IsNullOrWhiteSpace(productName))
            {
                var cleanWords = productName.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                var validLetters = new List<string>();
                foreach (var word in cleanWords)
                {
                    var lettersOnly = new string(word.Where(c => char.IsLetterOrDigit(c) && c < 128).ToArray());
                    if (!string.IsNullOrEmpty(lettersOnly))
                    {
                        validLetters.Add(lettersOnly);
                    }
                }

                if (validLetters.Count >= 2)
                {
                    var w1 = validLetters[0];
                    var w2 = validLetters[1];
                    middle = (w1[0].ToString() + (w2.Length > 1 ? w2.Substring(0, 2) : w2)).ToUpperInvariant();
                }
                else if (validLetters.Count == 1 && validLetters[0].Length >= 2)
                {
                    middle = validLetters[0].Substring(0, Math.Min(3, validLetters[0].Length)).ToUpperInvariant();
                }
            }

            var random = new Random();
            string candidateSku;
            int attempts = 0;
            do
            {
                int code = random.Next(1000, 9999);
                candidateSku = string.IsNullOrEmpty(middle) ? $"{prefix}-{code}" : $"{prefix}-{middle}-{code}";
                attempts++;
                if (attempts > 50)
                {
                    candidateSku = $"{prefix}-{DateTime.UtcNow:yyMMddHHmmss}";
                    break;
                }
            } while (await _context.Products.AnyAsync(p => p.SKU == candidateSku));

            return candidateSku;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            product.IsActive = !product.IsActive;
            product.UpdatedAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Product status updated to {(product.IsActive ? "Active" : "Archived")}.";
            return RedirectToAction(nameof(Index));
        }
    }
}
