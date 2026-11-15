using AarMaabd.Data;
using AarMaabd.Models;
using AarMaabd.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AarMaabd.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IOrderService _orderService;

        public OrdersController(ApplicationDbContext context, IOrderService orderService)
        {
            _context = context;
            _orderService = orderService;
        }

        public async Task<IActionResult> Index(OrderStatus? status, string? search)
        {
            var query = _context.Orders
                .Include(o => o.Items)
                .AsNoTracking();

            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(o => o.OrderNumber.Contains(term) || 
                                         o.CustomerName.Contains(term) || 
                                         o.CustomerPhone.Contains(term));
            }

            var orders = await query.OrderByDescending(o => o.CreatedAtUtc).ToListAsync();

            ViewBag.SelectedStatus = status;
            ViewBag.Search = search;

            return View(orders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .Include(o => o.StatusHistory.OrderByDescending(h => h.ChangedAtUtc))
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int orderId, OrderStatus newStatus, string? adminNotes)
        {
            var success = await _orderService.UpdateOrderStatusAsync(orderId, newStatus, User.Identity?.Name ?? "Admin", adminNotes);
            if (!success) return NotFound();

            TempData["SuccessMessage"] = $"Order status successfully updated to {newStatus}.";
            return RedirectToAction(nameof(Details), new { id = orderId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePayment(int orderId, PaymentStatus paymentStatus, string? transactionId)
        {
            var success = await _orderService.UpdatePaymentStatusAsync(orderId, paymentStatus, transactionId, User.Identity?.Name ?? "Admin");
            if (!success) return NotFound();

            TempData["SuccessMessage"] = $"Payment status updated to {paymentStatus}.";
            return RedirectToAction(nameof(Details), new { id = orderId });
        }

        public async Task<IActionResult> Invoice(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            return View(order);
        }
    }
}
