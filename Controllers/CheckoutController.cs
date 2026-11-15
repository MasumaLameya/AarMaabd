using AarMaabd.Models;
using AarMaabd.Services.Interfaces;
using AarMaabd.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AarMaabd.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CheckoutController(
            ICartService cartService,
            IOrderService orderService,
            UserManager<ApplicationUser> userManager)
        {
            _cartService = cartService;
            _orderService = orderService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string deliveryZone = "Inside Dhaka")
        {
            var coupon = HttpContext.Session.GetString("AppliedCouponCode");
            var summary = await _cartService.GetCartSummaryAsync(coupon, deliveryZone);

            if (!summary.Items.Any())
            {
                TempData["ErrorMessage"] = "Your cart is empty. Please add items before checking out.";
                return RedirectToAction("Index", "Cart");
            }

            var model = new CheckoutViewModel
            {
                CartSummary = summary,
                AppliedCouponCode = coupon,
                DeliveryZone = deliveryZone
            };

            // Pre-fill user data if logged in
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    model.CustomerName = user.FullName;
                    model.CustomerEmail = user.Email;
                    model.CustomerPhone = user.PhoneNumber ?? string.Empty;
                    if (!string.IsNullOrEmpty(user.Division)) model.Division = user.Division;
                    if (!string.IsNullOrEmpty(user.District)) model.District = user.District;
                    if (!string.IsNullOrEmpty(user.UpazilaThana)) model.UpazilaThana = user.UpazilaThana;
                    if (!string.IsNullOrEmpty(user.Area)) model.Area = user.Area;
                    if (!string.IsNullOrEmpty(user.StreetAddress)) model.StreetAddress = user.StreetAddress;
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            var coupon = HttpContext.Session.GetString("AppliedCouponCode");
            model.AppliedCouponCode = coupon;
            var summary = await _cartService.GetCartSummaryAsync(coupon, model.DeliveryZone);
            model.CartSummary = summary;

            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _orderService.PlaceOrderAsync(model, userId);

            if (!result.Success || result.Order == null)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View("Index", model);
            }

            // Remove coupon from session once order is placed
            HttpContext.Session.Remove("AppliedCouponCode");

            if (model.PaymentMethod == PaymentMethod.OnlinePayment)
            {
                return RedirectToAction("GatewaySimulator", new { orderNumber = result.Order.OrderNumber });
            }

            TempData["SuccessMessage"] = "Your order has been placed successfully!";
            return RedirectToAction("Confirmation", new { orderNumber = result.Order.OrderNumber });
        }

        [HttpGet]
        public async Task<IActionResult> Confirmation(string orderNumber)
        {
            if (string.IsNullOrWhiteSpace(orderNumber)) return RedirectToAction("Index", "Home");

            var order = await _orderService.GetOrderByNumberAsync(orderNumber);
            if (order == null) return NotFound();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!order.IsGuestOrder && order.UserId != currentUserId && !User.IsInRole("Admin"))
            {
                return NotFound();
            }

            var model = new OrderConfirmationViewModel
            {
                OrderNumber = order.OrderNumber,
                OrderDate = order.CreatedAtUtc,
                CustomerName = order.CustomerName,
                CustomerPhone = order.CustomerPhone,
                DeliveryAddress = $"{order.StreetAddress}, {order.Area}, {order.UpazilaThana}, {order.District}, {order.Division}",
                DeliveryZone = order.DeliveryZone,
                RequestedDeliveryDate = order.RequestedDeliveryDate,
                SubTotal = order.SubTotal,
                DiscountAmount = order.OfferDiscountAmount + order.CouponDiscountAmount,
                DeliveryFee = order.DeliveryFee,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                PaymentStatus = order.PaymentStatus,
                PaymentMethod = order.PaymentMethod,
                Items = order.Items.Select(i => new OrderItemViewModel
                {
                    ProductName = i.ProductNameSnapshot,
                    Sku = i.SkuSnapshot,
                    ImageUrl = i.ImageUrlSnapshot,
                    VariantName = i.VariantNameSnapshot,
                    CustomizationNote = i.CustomizationNoteSnapshot,
                    UnitPrice = i.UnitPriceSnapshot,
                    Quantity = i.Quantity
                }).ToList()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GatewaySimulator(string orderNumber)
        {
            var order = await _orderService.GetOrderByNumberAsync(orderNumber);
            if (order == null) return NotFound();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!order.IsGuestOrder && order.UserId != currentUserId && !User.IsInRole("Admin"))
            {
                return NotFound();
            }

            if (order.PaymentStatus == PaymentStatus.Paid)
            {
                return RedirectToAction("Confirmation", new { orderNumber = order.OrderNumber });
            }

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessOnlinePayment(string orderNumber, string gateway, string transactionId)
        {
            var order = await _orderService.GetOrderByNumberAsync(orderNumber);
            if (order == null) return NotFound();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!order.IsGuestOrder && order.UserId != currentUserId && !User.IsInRole("Admin"))
            {
                return NotFound();
            }

            // Prevent payment replay
            if (order.PaymentStatus == PaymentStatus.Paid)
            {
                TempData["SuccessMessage"] = "This order has already been verified as Paid.";
                return RedirectToAction("Confirmation", new { orderNumber = order.OrderNumber });
            }

            var trx = string.IsNullOrWhiteSpace(transactionId) ? $"TRX-{Guid.NewGuid().ToString("N")[..10].ToUpper()}" : transactionId.Trim();

            await _orderService.UpdatePaymentStatusAsync(order.Id, PaymentStatus.Paid, trx, $"Gateway: {gateway}");
            await _orderService.UpdateOrderStatusAsync(order.Id, OrderStatus.Confirmed, "Payment Verification", "Verified online payment received");

            TempData["SuccessMessage"] = $"Online payment verified successfully via {gateway}!";
            return RedirectToAction("Confirmation", new { orderNumber = order.OrderNumber });
        }
    }
}
