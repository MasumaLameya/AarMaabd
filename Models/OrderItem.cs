using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AarMaabd.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order? Order { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        // Immutable product snapshots at purchase time
        [Required, MaxLength(200)]
        public string ProductNameSnapshot { get; set; } = string.Empty;

        [MaxLength(60)]
        public string? SkuSnapshot { get; set; }

        [MaxLength(300)]
        public string? ImageUrlSnapshot { get; set; }

        [MaxLength(100)]
        public string? VariantNameSnapshot { get; set; }

        [MaxLength(250)]
        public string? CustomizationNoteSnapshot { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 100000000, ErrorMessage = "Unit price cannot be negative.")]
        public decimal UnitPriceSnapshot { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 100000000, ErrorMessage = "Discount applied cannot be negative.")]
        public decimal DiscountAppliedSnapshot { get; set; } = 0.00m;

        [Range(1, 100000, ErrorMessage = "Quantity must be at least 1 and cannot be negative.")]
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 100000000, ErrorMessage = "Line total cannot be negative.")]
        public decimal LineTotal { get; set; }
    }

    public class OrderStatusHistory
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order? Order { get; set; }

        public OrderStatus Status { get; set; }

        [Required, MaxLength(100)]
        public string ChangedBy { get; set; } = "System";

        [MaxLength(300)]
        public string? Note { get; set; }

        public DateTime ChangedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
