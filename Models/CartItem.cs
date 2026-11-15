using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AarMaabd.Models
{
    public class CartItem
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string CartSessionId { get; set; } = string.Empty;

        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public int? VariantId { get; set; }
        public ProductVariant? Variant { get; set; }

        [Range(1, 9999, ErrorMessage = "Quantity must be at least 1 and cannot be negative.")]
        public int Quantity { get; set; } = 1;

        [MaxLength(250)]
        public string? CustomizationNote { get; set; } // e.g. "Name on cake: Happy Birthday Sarah", "Initial: A"

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }
    }

    public class WishlistItem
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public DateTime AddedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
