using AarMaabd.Data;
using AarMaabd.Models;
using AarMaabd.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AarMaabd.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var now = DateTime.UtcNow;

            var banners = await _context.Banners
                .AsNoTracking()
                .Where(b => b.IsActive)
                .OrderBy(b => b.DisplayOrder)
                .ToListAsync();

            var categories = await _context.Categories
                .AsNoTracking()
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            var featuredProducts = await _context.Products
                .Include(p => p.Images)
                .Include(p => p.Category)
                .AsNoTracking()
                .Where(p => p.IsActive && p.IsFeatured)
                .Take(8)
                .ToListAsync();

            var bestSellers = await _context.Products
                .Include(p => p.Images)
                .Include(p => p.Category)
                .AsNoTracking()
                .Where(p => p.IsActive && p.IsBestSeller)
                .Take(8)
                .ToListAsync();

            var newArrivals = await _context.Products
                .Include(p => p.Images)
                .Include(p => p.Category)
                .AsNoTracking()
                .Where(p => p.IsActive && p.IsNewArrival)
                .Take(8)
                .ToListAsync();

            var offers = await _context.Offers
                .AsNoTracking()
                .Where(o => o.IsActive && o.StartDateUtc <= now && o.EndDateUtc >= now)
                .OrderBy(o => o.DisplayOrder)
                .ToListAsync();

            var announcementSetting = await _context.StoreSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Key == "Store:Announcement");

            var model = new HomeViewModel
            {
                HeroBanners = banners,
                Categories = categories,
                FeaturedProducts = featuredProducts,
                BestSellers = bestSellers.Any() ? bestSellers : featuredProducts,
                NewArrivals = newArrivals.Any() ? newArrivals : featuredProducts,
                ActiveOffers = offers,
                AnnouncementText = announcementSetting?.Value ?? "🌸 Welcome to AarMaabd - Handmade With Love"
            };

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int? statusCode = null)
        {
            ViewBag.StatusCode = statusCode;
            return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
