using AarMaabd.Data;
using AarMaabd.Models;
using AarMaabd.Services.Interfaces;
using AarMaabd.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AarMaabd.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICartService _cartService;
        private readonly IPromotionService _promotionService;

        public OrderService(
            ApplicationDbContext context,
            ICartService cartService,
            IPromotionService promotionService)
        {
            _context = context;
            _cartService = cartService;
            _promotionService = promotionService;
        }

        public async Task<OrderPlacementResult> PlaceOrderAsync(CheckoutViewModel model, string? userId)
        {
            var cartSummary = await _cartService.GetCartSummaryAsync(model.AppliedCouponCode, model.DeliveryZone);
            if (!cartSummary.Items.Any())
            {
                return new OrderPlacementResult { Success = false, Message = "Your shopping cart is currently empty." };
            }

            // Begin atomic transaction
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                decimal recomputedSubTotal = 0.00m;
                var orderItems = new List<OrderItem>();

                // Atomically verify stock and build snapshot
                foreach (var cartItem in cartSummary.Items)
                {
                    var product = await _context.Products
                        .Include(p => p.Images)
                        .FirstOrDefaultAsync(p => p.Id == cartItem.ProductId);

                    if (product == null || !product.IsActive)
                    {
                        await transaction.RollbackAsync();
                        return new OrderPlacementResult 
                        { 
                            Success = false, 
                            Message = $"The item '{cartItem.ProductName}' is no longer available." 
                        };
                    }

                    if (product.ProductType == ProductType.ReadyStock && product.StockQuantity < cartItem.Quantity)
                    {
                        await transaction.RollbackAsync();
                        return new OrderPlacementResult 
                        { 
                            Success = false, 
                            Message = $"Sorry, only {product.StockQuantity} units of '{product.Name}' are left in stock." 
                        };
                    }

                    // Decrement stock atomically
                    if (product.ProductType == ProductType.ReadyStock)
                    {
                        product.StockQuantity -= cartItem.Quantity;
                    }

                    var effectiveUnitPrice = product.CurrentEffectivePrice;
                    var lineTotal = effectiveUnitPrice * cartItem.Quantity;
                    recomputedSubTotal += lineTotal;

                    orderItems.Add(new OrderItem
                    {
                        ProductId = product.Id,
                        ProductNameSnapshot = product.Name,
                        SkuSnapshot = product.SKU,
                        ImageUrlSnapshot = product.PrimaryImageUrl,
                        VariantNameSnapshot = cartItem.VariantName,
                        CustomizationNoteSnapshot = cartItem.CustomizationNote,
                        UnitPriceSnapshot = effectiveUnitPrice,
                        DiscountAppliedSnapshot = product.Price - effectiveUnitPrice,
                        Quantity = cartItem.Quantity,
                        LineTotal = lineTotal
                    });
                }

                // Coupon evaluation
                decimal couponDiscount = 0.00m;
                int? appliedCouponId = null;
                if (!string.IsNullOrWhiteSpace(model.AppliedCouponCode))
                {
                    var customerId = userId ?? model.CustomerPhone;
                    var couponResult = await _promotionService.ValidateCouponAsync(model.AppliedCouponCode, recomputedSubTotal, customerId);
                    if (couponResult.IsValid && couponResult.Coupon != null)
                    {
                        couponDiscount = couponResult.DiscountAmount;
                        appliedCouponId = couponResult.Coupon.Id;
                    }
                }

                // Delivery Zone validation & fee calculation: Delivery is strictly inside Dhaka only
                if (!string.Equals(model.Division, "Dhaka", StringComparison.OrdinalIgnoreCase) || 
                    model.DeliveryZone == "Outside Dhaka")
                {
                    await transaction.RollbackAsync();
                    return new OrderPlacementResult
                    {
                        Success = false,
                        Message = "We currently deliver only inside Dhaka. Please select a delivery address inside Dhaka."
                    };
                }

                model.DeliveryZone = "Inside Dhaka";
                decimal deliveryFee = 0.00m;
                if (recomputedSubTotal < 2500.00m) // Free delivery threshold
                {
                    deliveryFee = 70.00m;
                }

                decimal totalPayable = Math.Max(0, recomputedSubTotal - couponDiscount + deliveryFee);

                // Generate Unique Order Reference
                var randomPart = new Random().Next(1000, 9999);
                var orderNumber = $"AAR-{DateTime.UtcNow:yyyyMMdd}-{randomPart}";

                var order = new Order
                {
                    OrderNumber = orderNumber,
                    UserId = userId,
                    IsGuestOrder = string.IsNullOrEmpty(userId),
                    CustomerName = model.CustomerName,
                    CustomerPhone = model.CustomerPhone,
                    CustomerEmail = model.CustomerEmail,
                    Division = model.Division,
                    District = model.District,
                    UpazilaThana = model.UpazilaThana,
                    Area = model.Area,
                    StreetAddress = model.StreetAddress,
                    Landmark = model.Landmark,
                    DeliveryNotes = model.DeliveryNotes,
                    RequestedDeliveryDate = model.RequestedDeliveryDate,
                    DeliveryZone = model.DeliveryZone,
                    SubTotal = recomputedSubTotal,
                    CouponDiscountAmount = couponDiscount,
                    AppliedCouponCode = model.AppliedCouponCode,
                    DeliveryFee = deliveryFee,
                    TotalAmount = totalPayable,
                    Status = OrderStatus.Pending,
                    PaymentStatus = model.PaymentMethod == PaymentMethod.CashOnDelivery ? PaymentStatus.Unpaid : PaymentStatus.Pending,
                    PaymentMethod = model.PaymentMethod,
                    CreatedAtUtc = DateTime.UtcNow,
                    Items = orderItems
                };

                order.StatusHistory.Add(new OrderStatusHistory
                {
                    Status = OrderStatus.Pending,
                    ChangedBy = "Customer Checkout",
                    Note = $"Order placed with {model.PaymentMethod}. Payable: ৳{totalPayable:N0}",
                    ChangedAtUtc = DateTime.UtcNow
                });

                await _context.Orders.AddAsync(order);
                await _context.SaveChangesAsync();

                // If coupon was applied, record usage
                if (appliedCouponId.HasValue && couponDiscount > 0)
                {
                    await _promotionService.RecordCouponUsageAsync(
                        appliedCouponId.Value, 
                        userId ?? model.CustomerPhone, 
                        order.Id, 
                        couponDiscount);
                }

                await transaction.CommitAsync();

                // Clear customer's shopping cart after successful order
                await _cartService.ClearCartAsync();

                return new OrderPlacementResult
                {
                    Success = true,
                    Order = order,
                    Message = "Your order has been placed successfully!"
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new OrderPlacementResult
                {
                    Success = false,
                    Message = $"An error occurred while placing your order: {ex.Message}"
                };
            }
        }

        public async Task<Order?> GetOrderByNumberAsync(string orderNumber)
        {
            return await _context.Orders
                .Include(o => o.Items)
                .Include(o => o.StatusHistory.OrderByDescending(h => h.ChangedAtUtc))
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
        }

        public async Task<Order?> GetOrderForCustomerAsync(string orderNumber, string customerPhone)
        {
            var normalizedPhone = customerPhone.Replace(" ", "").Replace("-", "");
            return await _context.Orders
                .Include(o => o.Items)
                .Include(o => o.StatusHistory.OrderByDescending(h => h.ChangedAtUtc))
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber && 
                    (o.CustomerPhone.Contains(normalizedPhone) || normalizedPhone.Contains(o.CustomerPhone)));
        }

        public async Task<List<Order>> GetCustomerOrdersAsync(string userId)
        {
            return await _context.Orders
                .Include(o => o.Items)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAtUtc)
                .ToListAsync();
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus, string adminUser, string? notes = null)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return false;

            order.Status = newStatus;
            if (newStatus == OrderStatus.Confirmed && !order.ConfirmedAtUtc.HasValue) order.ConfirmedAtUtc = DateTime.UtcNow;
            if (newStatus == OrderStatus.Shipped && !order.ShippedAtUtc.HasValue) order.ShippedAtUtc = DateTime.UtcNow;
            if (newStatus == OrderStatus.Delivered && !order.DeliveredAtUtc.HasValue)
            {
                order.DeliveredAtUtc = DateTime.UtcNow;
                if (order.PaymentMethod == PaymentMethod.CashOnDelivery)
                {
                    order.PaymentStatus = PaymentStatus.Paid;
                }
            }
            if (newStatus == OrderStatus.Cancelled && !order.CancelledAtUtc.HasValue)
            {
                order.CancelledAtUtc = DateTime.UtcNow;
                order.CancellationReason = notes;
            }

            order.StatusHistory.Add(new OrderStatusHistory
            {
                OrderId = orderId,
                Status = newStatus,
                ChangedBy = adminUser,
                Note = notes ?? $"Status updated to {newStatus}",
                ChangedAtUtc = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdatePaymentStatusAsync(int orderId, PaymentStatus newStatus, string? transactionId = null, string? adminUser = null)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return false;

            order.PaymentStatus = newStatus;
            if (!string.IsNullOrWhiteSpace(transactionId))
            {
                order.PaymentTransactionId = transactionId;
            }

            order.StatusHistory.Add(new OrderStatusHistory
            {
                OrderId = orderId,
                Status = order.Status,
                ChangedBy = adminUser ?? "Payment Gateway Verification",
                Note = $"Payment marked as {newStatus}. Transaction: {transactionId ?? "N/A"}",
                ChangedAtUtc = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
