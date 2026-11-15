using AarMaabd.Models;

namespace AarMaabd.ViewModels
{
    public class CartItemViewModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSlug { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public decimal? OriginalPrice { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal => UnitPrice * Quantity;
        public string? VariantName { get; set; }
        public string? CustomizationNote { get; set; }
        public int AvailableStock { get; set; }
    }

    public class CartSummaryViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new();
        public int TotalItemsCount => Items.Sum(i => i.Quantity);
        public decimal SubTotal => Items.Sum(i => i.LineTotal);
        public decimal OfferDiscount { get; set; } = 0.00m;
        public decimal CouponDiscount { get; set; } = 0.00m;
        public string? AppliedCouponCode { get; set; }
        public string? CouponDescription { get; set; }
        public decimal EstimatedDeliveryFee { get; set; } = 70.00m;
        public decimal GrandTotal => Math.Max(0, SubTotal - OfferDiscount - CouponDiscount + EstimatedDeliveryFee);
        public string DeliveryZone { get; set; } = "Inside Dhaka";
        public bool IsEligibleForFreeDelivery => SubTotal >= 2500.00m;
    }

    public class AddToCartRequest
    {
        public int ProductId { get; set; }

        [System.ComponentModel.DataAnnotations.Range(1, 9999, ErrorMessage = "Quantity must be at least 1 and cannot be negative.")]
        public int Quantity { get; set; } = 1;
        public int? VariantId { get; set; }
        public string? CustomizationNote { get; set; }
    }

    public class UpdateCartRequest
    {
        public int CartItemId { get; set; }

        [System.ComponentModel.DataAnnotations.Range(0, 9999, ErrorMessage = "Quantity cannot be negative.")]
        public int Quantity { get; set; }
    }

    public class ApplyCouponRequest
    {
        public string CouponCode { get; set; } = string.Empty;
    }
}
