using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AarMaabd.Models
{
    public enum DiscountType
    {
        Percentage = 1,
        FixedAmount = 2
    }

    public enum OfferTargetType
    {
        AllProducts = 1,
        Category = 2,
        Product = 3
    }

    public class Offer
    {
        public int Id { get; set; }

        [Required, MaxLength(120)]
        public string Title { get; set; } = string.Empty; // e.g. "Eid Special 20% Off"

        [MaxLength(50)]
        public string? BadgeText { get; set; } // e.g. "20% OFF", "Save ৳200"

        public DiscountType DiscountType { get; set; } = DiscountType.Percentage;

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 10000000, ErrorMessage = "Discount value cannot be negative.")]
        public decimal DiscountValue { get; set; } // e.g. 10, 20 or fixed BDT 150

        public OfferTargetType TargetType { get; set; } = OfferTargetType.AllProducts;

        public int? TargetCategoryId { get; set; }
        public Category? TargetCategory { get; set; }

        public int? TargetProductId { get; set; }
        public Product? TargetProduct { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 10000000, ErrorMessage = "Minimum order amount cannot be negative.")]
        public decimal MinOrderAmount { get; set; } = 0.00m;

        public DateTime StartDateUtc { get; set; } = DateTime.UtcNow;
        public DateTime EndDateUtc { get; set; } = DateTime.UtcNow.AddMonths(1);

        public bool IsActive { get; set; } = true;

        [Range(0, 100000, ErrorMessage = "Display order cannot be negative.")]
        public int DisplayOrder { get; set; } = 0;

        [NotMapped]
        public bool IsCurrentlyValid => IsActive && DateTime.UtcNow >= StartDateUtc && DateTime.UtcNow <= EndDateUtc;
    }
}
