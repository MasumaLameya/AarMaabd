using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AarMaabd.Models
{
    public class Coupon
    {
        public int Id { get; set; }

        [Required, MaxLength(30)]
        public string Code { get; set; } = string.Empty; // Normalized uppercase, e.g. "AAR10", "WELCOME20"

        [MaxLength(200)]
        public string? Description { get; set; }

        public DiscountType DiscountType { get; set; } = DiscountType.Percentage;

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 10000000, ErrorMessage = "Discount value cannot be negative.")]
        public decimal DiscountValue { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 10000000, ErrorMessage = "Maximum discount cap cannot be negative.")]
        public decimal? MaxDiscountCap { get; set; } // Maximum BDT limit, e.g. cap at ৳500

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 10000000, ErrorMessage = "Minimum spend amount cannot be negative.")]
        public decimal MinSpendAmount { get; set; } = 0.00m; // Minimum cart subtotal required

        public DateTime StartDateUtc { get; set; } = DateTime.UtcNow;
        public DateTime EndDateUtc { get; set; } = DateTime.UtcNow.AddMonths(1);

        public bool IsActive { get; set; } = true;

        [Range(0, int.MaxValue, ErrorMessage = "Total usage limit cannot be negative.")]
        public int? UsageLimitTotal { get; set; } = 100;

        [Range(0, int.MaxValue, ErrorMessage = "Total usage count cannot be negative.")]
        public int UsageCountTotal { get; set; } = 0;

        [Range(0, int.MaxValue, ErrorMessage = "Per customer usage limit cannot be negative.")]
        public int UsageLimitPerCustomer { get; set; } = 1;
        public bool FirstOrderOnly { get; set; } = false;

        public ICollection<CouponUsage> Usages { get; set; } = new List<CouponUsage>();

        [NotMapped]
        public bool IsCurrentlyValid => IsActive 
            && DateTime.UtcNow >= StartDateUtc 
            && DateTime.UtcNow <= EndDateUtc
            && (!UsageLimitTotal.HasValue || UsageCountTotal < UsageLimitTotal.Value);
    }

    public class CouponUsage
    {
        public int Id { get; set; }

        public int CouponId { get; set; }
        public Coupon? Coupon { get; set; }

        [Required]
        public string CustomerIdentifier { get; set; } = string.Empty; // User ID or Customer Phone for guest

        public int OrderId { get; set; }
        public Order? Order { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 10000000, ErrorMessage = "Discount applied cannot be negative.")]
        public decimal DiscountAmountApplied { get; set; }

        public DateTime UsedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
