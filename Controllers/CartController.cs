using AarMaabd.Services.Interfaces;
using AarMaabd.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace AarMaabd.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IPromotionService _promotionService;

        public CartController(ICartService cartService, IPromotionService promotionService)
        {
            _cartService = cartService;
            _promotionService = promotionService;
        }

        public async Task<IActionResult> Index()
        {
            var coupon = HttpContext.Session.GetString("AppliedCouponCode");
            var summary = await _cartService.GetCartSummaryAsync(coupon, "Inside Dhaka");
            return View(summary);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            if (request == null || request.ProductId <= 0)
            {
                return Json(new { success = false, message = "Invalid product." });
            }

            if (request.Quantity <= 0)
            {
                request.Quantity = 1;
            }

            await _cartService.AddToCartAsync(request);
            var count = await _cartService.GetCartItemCountAsync();

            return Json(new { success = true, message = "Added to cart successfully!", cartCount = count });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity([FromBody] UpdateCartRequest request)
        {
            if (request == null || request.CartItemId <= 0)
            {
                return Json(new { success = false, message = "Invalid cart item." });
            }

            if (request.Quantity < 0)
            {
                return Json(new { success = false, message = "Quantity cannot be negative." });
            }

            await _cartService.UpdateQuantityAsync(request.CartItemId, request.Quantity);
            var coupon = HttpContext.Session.GetString("AppliedCouponCode");
            var summary = await _cartService.GetCartSummaryAsync(coupon);

            return Json(new 
            { 
                success = true, 
                cartCount = summary.TotalItemsCount,
                subTotal = summary.SubTotal,
                couponDiscount = summary.CouponDiscount,
                estimatedDeliveryFee = summary.EstimatedDeliveryFee,
                grandTotal = summary.GrandTotal,
                isFreeDelivery = summary.IsEligibleForFreeDelivery
            });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveItem(int id)
        {
            await _cartService.RemoveFromCartAsync(id);
            TempData["SuccessMessage"] = "Item removed from cart.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ApplyCoupon([FromBody] ApplyCouponRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.CouponCode))
            {
                return Json(new { success = false, message = "Please enter a coupon code." });
            }

            var summary = await _cartService.GetCartSummaryAsync();
            var customerId = User.Identity?.Name ?? _cartService.GetOrCreateCartSessionId();

            var result = await _promotionService.ValidateCouponAsync(request.CouponCode, summary.SubTotal, customerId);
            if (!result.IsValid)
            {
                return Json(new { success = false, message = result.Message });
            }

            HttpContext.Session.SetString("AppliedCouponCode", result.Coupon!.Code);

            var updatedSummary = await _cartService.GetCartSummaryAsync(result.Coupon.Code);

            return Json(new
            {
                success = true,
                message = result.Message,
                code = result.Coupon.Code,
                discount = result.DiscountAmount,
                grandTotal = updatedSummary.GrandTotal
            });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveCoupon()
        {
            HttpContext.Session.Remove("AppliedCouponCode");
            var summary = await _cartService.GetCartSummaryAsync();
            return Json(new
            {
                success = true,
                message = "Coupon removed.",
                grandTotal = summary.GrandTotal
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetCount()
        {
            var count = await _cartService.GetCartItemCountAsync();
            return Json(new { count });
        }
    }
}
