using AarMaabd.Models;

namespace AarMaabd.Services.Interfaces
{
    public class CouponValidationResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
        public Coupon? Coupon { get; set; }
        public decimal DiscountAmount { get; set; }
    }

    public interface IPromotionService
    {
        Task<List<Offer>> GetActiveOffersAsync();
        Task<CouponValidationResult> ValidateCouponAsync(string couponCode, decimal eligibleSubtotal, string customerIdentifier);
        Task RecordCouponUsageAsync(int couponId, string customerIdentifier, int orderId, decimal discountAmount);
    }
}
