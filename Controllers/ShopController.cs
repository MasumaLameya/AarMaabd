using AarMaabd.Data;
using AarMaabd.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AarMaabd.Controllers
{
    public class ShopController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ShopController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
            string? category, 
            string? subcategory,
            string? q, 
            string? sort = "featured", 
            decimal? minPrice = null, 
            decimal? maxPrice = null, 
            bool? inStock = null, 
            int page = 1)
        {
            const int pageSize = 12;
            if (page < 1) page = 1;

            var query = _context.Products
                .Include(p => p.Images)
                .Include(p => p.Category)
                .Include(p => p.SubCategory)
                .AsNoTracking()
                .Where(p => p.IsActive);

            // Filter Category
            string? selectedCategoryName = null;
            int? selectedCategoryId = null;

            if (!string.IsNullOrWhiteSpace(category))
            {
                var catEntity = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Slug == category || c.Name.ToLower() == category.ToLower());

                if (catEntity != null)
                {
                    query = query.Where(p => p.CategoryId == catEntity.Id);
                    selectedCategoryId = catEntity.Id;
                    selectedCategoryName = catEntity.Name;
                }
            }

            // Filter SubCategory
            int? selectedSubCategoryId = null;
            if (!string.IsNullOrWhiteSpace(subcategory))
            {
                var subEntity = await _context.SubCategories
                    .FirstOrDefaultAsync(s => s.Slug == subcategory || s.Name.ToLower() == subcategory.ToLower());

                if (subEntity != null)
                {
                    query = query.Where(p => p.SubCategoryId == subEntity.Id);
                    selectedSubCategoryId = subEntity.Id;
                }
            }

            // Search Query
            if (!string.IsNullOrWhiteSpace(q))
            {
                var searchTerm = q.Trim().ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(searchTerm) || 
                                         (p.ShortDescription != null && p.ShortDescription.ToLower().Contains(searchTerm)) ||
                                         p.SKU.ToLower().Contains(searchTerm));
            }

            // Price filtering (ensure never negative)
            if (minPrice.HasValue)
            {
                var safeMin = Math.Max(0, minPrice.Value);
                query = query.Where(p => (p.SalePrice ?? p.Price) >= safeMin);
            }

            if (maxPrice.HasValue)
            {
                var safeMax = Math.Max(0, maxPrice.Value);
                query = query.Where(p => (p.SalePrice ?? p.Price) <= safeMax);
            }

            // In Stock filtering
            if (inStock.HasValue && inStock.Value)
            {
                query = query.Where(p => p.StockQuantity > 0);
            }

            // Sorting
            query = sort switch
            {
                "latest" => query.OrderByDescending(p => p.CreatedAtUtc),
                "price_asc" => query.OrderBy(p => p.SalePrice ?? p.Price),
                "price_desc" => query.OrderByDescending(p => p.SalePrice ?? p.Price),
                _ => query.OrderByDescending(p => p.IsFeatured).ThenByDescending(p => p.CreatedAtUtc)
            };

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var categories = await _context.Categories
                .Include(c => c.SubCategories)
                .AsNoTracking()
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            var model = new ShopViewModel
            {
                Categories = categories,
                Products = products,
                SelectedCategoryId = selectedCategoryId,
                SelectedSubCategoryId = selectedSubCategoryId,
                SelectedCategorySlug = category,
                SelectedCategoryName = selectedCategoryName,
                SearchQuery = q,
                SortBy = sort,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                InStockOnly = inStock,
                CurrentPage = page,
                TotalPages = totalPages > 0 ? totalPages : 1,
                TotalProducts = totalCount
            };

            return View(model);
        }
    }
}
