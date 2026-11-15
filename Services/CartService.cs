using AarMaabd.Data;
using AarMaabd.Models;
using AarMaabd.Services.Interfaces;
using AarMaabd.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AarMaabd.Services
{
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPromotionService _promotionService;
        private const string CartSessionCookieKey = "aarmaabd_cart_id";

        public CartService(
            ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor,
            IPromotionService promotionService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _promotionService = promotionService;
        }

        public string GetOrCreateCartSessionId()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return Guid.NewGuid().ToString("N");

            if (httpContext.Request.Cookies.TryGetValue(CartSessionCookieKey, out var existingSessionId) 
                && !string.IsNullOrWhiteSpace(existingSessionId))
            {
                return existingSessionId;
            }

            var newSessionId = Guid.NewGuid().ToString("N");
            httpContext.Response.Cookies.Append(CartSessionCookieKey, newSessionId, new CookieOptions
            {
                HttpOnly = true,
                Secure = httpContext.Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(30)
            });

            return newSessionId;
        }

        private string? GetCurrentUserId()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        public async Task<int> GetCartItemCountAsync()
        {
            var sessionId = GetOrCreateCartSessionId();
            var userId = GetCurrentUserId();

            return await _context.CartItems
                .Where(c => (userId != null && c.UserId == userId) || c.CartSessionId == sessionId)
                .SumAsync(c => c.Quantity);
        }

        public async Task<CartSummaryViewModel> GetCartSummaryAsync(string? appliedCouponCode = null, string deliveryZone = "Inside Dhaka")
        {
            var sessionId = GetOrCreateCartSessionId();
            var userId = GetCurrentUserId();

            var cartQuery = _context.CartItems
                .Include(c => c.Product)
                    .ThenInclude(p => p!.Images)
                .Include(c => c.Variant)
                .Where(c => (userId != null && c.UserId == userId) || c.CartSessionId == sessionId);

            var dbCartItems = await cartQuery.ToListAsync();

            var summary = new CartSummaryViewModel
            {
                DeliveryZone = deliveryZone
            };

            foreach (var item in dbCartItems)
            {
                if (item.Product == null || !item.Product.IsActive) continue;

                // Server-authoritative pricing
                var unitPrice = item.Product.CurrentEffectivePrice;
                if (item.Variant != null && item.Variant.AdditionalPrice > 0)
                {
                    unitPrice += item.Variant.AdditionalPrice;
                }

                summary.Items.Add(new CartItemViewModel
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    ProductName = item.Product.Name,
                    ProductSlug = item.Product.Slug,
                    Sku = item.Product.SKU,
                    ImageUrl = item.Product.PrimaryImageUrl,
                    UnitPrice = unitPrice,
                    OriginalPrice = item.Product.Price,
                    Quantity = item.Quantity,
                    VariantName = item.Variant?.VariantName,
                    CustomizationNote = item.CustomizationNote,
                    AvailableStock = item.Product.StockQuantity
                });
            }

            // Delivery Fee rule: Delivery is only inside Dhaka (৳70 standard, FREE over ৳2,500)
            decimal baseDeliveryFee = 70.00m;
            summary.DeliveryZone = "Inside Dhaka";
            summary.EstimatedDeliveryFee = summary.IsEligibleForFreeDelivery ? 0.00m : baseDeliveryFee;

            // Coupon validation if requested
            if (!string.IsNullOrWhiteSpace(appliedCouponCode))
            {
                var customerId = userId ?? sessionId;
                var couponResult = await _promotionService.ValidateCouponAsync(appliedCouponCode, summary.SubTotal, customerId);
                if (couponResult.IsValid && couponResult.Coupon != null)
                {
                    summary.CouponDiscount = couponResult.DiscountAmount;
                    summary.AppliedCouponCode = couponResult.Coupon.Code;
                    summary.CouponDescription = couponResult.Message;
                }
            }

            return summary;
        }

        public async Task AddToCartAsync(AddToCartRequest request)
        {
            if (request.Quantity <= 0) request.Quantity = 1;

            var product = await _context.Products.FindAsync(request.ProductId);
            if (product == null || !product.IsActive) return;

            var sessionId = GetOrCreateCartSessionId();
            var userId = GetCurrentUserId();

            var existingItem = await _context.CartItems.FirstOrDefaultAsync(c =>
                ((userId != null && c.UserId == userId) || c.CartSessionId == sessionId) &&
                c.ProductId == request.ProductId &&
                c.VariantId == request.VariantId);

            if (existingItem != null)
            {
                existingItem.Quantity += request.Quantity;
                if (!string.IsNullOrWhiteSpace(request.CustomizationNote))
                {
                    existingItem.CustomizationNote = request.CustomizationNote;
                }
                existingItem.UpdatedAtUtc = DateTime.UtcNow;
            }
            else
            {
                var newItem = new CartItem
                {
                    CartSessionId = sessionId,
                    UserId = userId,
                    ProductId = request.ProductId,
                    VariantId = request.VariantId,
                    Quantity = request.Quantity,
                    CustomizationNote = request.CustomizationNote,
                    CreatedAtUtc = DateTime.UtcNow
                };
                await _context.CartItems.AddAsync(newItem);
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpdateQuantityAsync(int cartItemId, int quantity)
        {
            var sessionId = GetOrCreateCartSessionId();
            var userId = GetCurrentUserId();

            var item = await _context.CartItems.FirstOrDefaultAsync(c =>
                c.Id == cartItemId &&
                ((userId != null && c.UserId == userId) || c.CartSessionId == sessionId));

            if (item != null)
            {
                if (quantity <= 0)
                {
                    _context.CartItems.Remove(item);
                }
                else
                {
                    item.Quantity = quantity;
                    item.UpdatedAtUtc = DateTime.UtcNow;
                }
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveFromCartAsync(int cartItemId)
        {
            var sessionId = GetOrCreateCartSessionId();
            var userId = GetCurrentUserId();

            var item = await _context.CartItems.FirstOrDefaultAsync(c =>
                c.Id == cartItemId &&
                ((userId != null && c.UserId == userId) || c.CartSessionId == sessionId));

            if (item != null)
            {
                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();
            }
        }

        public async Task ClearCartAsync()
        {
            var sessionId = GetOrCreateCartSessionId();
            var userId = GetCurrentUserId();

            var items = await _context.CartItems
                .Where(c => (userId != null && c.UserId == userId) || c.CartSessionId == sessionId)
                .ToListAsync();

            if (items.Any())
            {
                _context.CartItems.RemoveRange(items);
                await _context.SaveChangesAsync();
            }
        }

        public async Task MigrateGuestCartToUserAsync(string userId)
        {
            var sessionId = GetOrCreateCartSessionId();
            var guestItems = await _context.CartItems
                .Where(c => c.CartSessionId == sessionId && c.UserId == null)
                .ToListAsync();

            foreach (var item in guestItems)
            {
                item.UserId = userId;
                item.UpdatedAtUtc = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }
    }
}
