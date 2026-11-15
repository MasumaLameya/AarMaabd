using AarMaabd.Data;
using AarMaabd.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AarMaabd.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.UtcNow.Date;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);

            var totalOrders = await _context.Orders.CountAsync();
            var pendingOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Pending);
            var confirmedOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Confirmed);
            var deliveredOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Delivered);

            var totalRevenue = await _context.Orders
                .Where(o => o.PaymentStatus == PaymentStatus.Paid || o.Status == OrderStatus.Delivered)
                .SumAsync(o => o.TotalAmount);

            var monthlyRevenue = await _context.Orders
                .Where(o => o.CreatedAtUtc >= startOfMonth && (o.PaymentStatus == PaymentStatus.Paid || o.Status == OrderStatus.Delivered))
                .SumAsync(o => o.TotalAmount);

            var totalProducts = await _context.Products.CountAsync(p => p.IsActive);
            var lowStockCount = await _context.Products.CountAsync(p => p.IsActive && p.StockQuantity <= p.LowStockThreshold);
            var pendingCustomOrders = await _context.CustomOrderRequests.CountAsync(c => c.Status == CustomOrderStatus.Requested);

            var recentOrders = await _context.Orders
                .Include(o => o.Items)
                .OrderByDescending(o => o.CreatedAtUtc)
                .Take(7)
                .ToListAsync();

            ViewBag.TotalOrders = totalOrders;
            ViewBag.PendingOrders = pendingOrders;
            ViewBag.ConfirmedOrders = confirmedOrders;
            ViewBag.DeliveredOrders = deliveredOrders;
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.MonthlyRevenue = monthlyRevenue;
            ViewBag.TotalProducts = totalProducts;
            ViewBag.LowStockCount = lowStockCount;
            ViewBag.PendingCustomOrders = pendingCustomOrders;

            return View(recentOrders);
        }
    }
}
