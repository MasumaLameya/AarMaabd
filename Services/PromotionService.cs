using AarMaabd.Data;
using AarMaabd.Models;
using AarMaabd.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AarMaabd.Services
{
    public class PromotionService : IPromotionService
    {
        private readonly ApplicationDbContext _context;

        public PromotionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Offer>> GetActiveOffersAsync()
        {
            var now = DateTime.UtcNow;
            return await _context.Offers
                .AsNoTracking()
                .Where(o => o.IsActive && o.StartDateUtc <= now && o.EndDateUtc >= now)
                .OrderBy(o => o.DisplayOrder)
                .ToListAsync();
        }

        public async Task<CouponValidationResult> ValidateCouponAsync(string couponCode, decimal eligibleSubtotal, string customerIdentifier)
        {
            if (string.IsNullOrWhiteSpace(couponCode))
            {
                return new CouponValidationResult { IsValid = false, Message = "Please enter a coupon code." };
            }

            var normalizedCode = couponCode.Trim().ToUpperInvariant();
            var now = DateTime.UtcNow;

            var coupon = await _context.Coupons
                .Include(c => c.Usages)
                .FirstOrDefaultAsync(c => c.Code == normalizedCode);

            if (coupon == null || !coupon.IsActive)
            {
                return new CouponValidationResult { IsValid = false, Message = "Invalid or expired coupon code." };
            }

            if (now < coupon.StartDateUtc || now > coupon.EndDateUtc)
            {
                return new CouponValidationResult { IsValid = false, Message = "This coupon code is currently not active or has expired." };
            }

            if (coupon.UsageLimitTotal.HasValue && coupon.UsageCountTotal >= coupon.UsageLimitTotal.Value)
            {
                return new CouponValidationResult { IsValid = false, Message = "This coupon code has reached its maximum total redemption limit." };
            }

            if (eligibleSubtotal < coupon.MinSpendAmount)
            {
                return new CouponValidationResult 
                { 
                    IsValid = false, 
                    Message = $"Minimum cart subtotal of ৳{coupon.MinSpendAmount:N0} is required to use this coupon." 
                };
            }

            if (!string.IsNullOrEmpty(customerIdentifier))
            {
                var userUsageCount = coupon.Usages.Count(u => u.CustomerIdentifier == customerIdentifier);
                if (userUsageCount >= coupon.UsageLimitPerCustomer)
                {
                    return new CouponValidationResult 
                    { 
                        IsValid = false, 
                        Message = "You have already used this coupon code the maximum allowed number of times." 
                    };
                }
            }

            // Calculate Discount
            decimal discount = 0.00m;
            if (coupon.DiscountType == DiscountType.Percentage)
            {
                discount = Math.Round(eligibleSubtotal * (coupon.DiscountValue / 100.00m), 2);
                if (coupon.MaxDiscountCap.HasValue && coupon.MaxDiscountCap.Value > 0)
                {
                    discount = Math.Min(discount, coupon.MaxDiscountCap.Value);
                }
            }
            else // FixedAmount
            {
                discount = Math.Min(coupon.DiscountValue, eligibleSubtotal);
            }

            discount = Math.Max(0.00m, discount);

            return new CouponValidationResult
            {
                IsValid = true,
                Coupon = coupon,
                DiscountAmount = discount,
                Message = $"Coupon applied! You saved ৳{discount:N0}."
            };
        }

        public async Task RecordCouponUsageAsync(int couponId, string customerIdentifier, int orderId, decimal discountAmount)
        {
            var coupon = await _context.Coupons.FindAsync(couponId);
            if (coupon != null)
            {
                coupon.UsageCountTotal += 1;
                var usage = new CouponUsage
                {
                    CouponId = couponId,
                    CustomerIdentifier = customerIdentifier,
                    OrderId = orderId,
                    DiscountAmountApplied = discountAmount,
                    UsedAtUtc = DateTime.UtcNow
                };
                await _context.CouponUsages.AddAsync(usage);
                await _context.SaveChangesAsync();
            }
        }
    }
}
