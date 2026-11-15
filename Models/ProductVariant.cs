using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AarMaabd.Models
{
    public class ProductVariant
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        [Required, MaxLength(100)]
        public string VariantName { get; set; } = string.Empty; // e.g. "Color: Lavender", "Flavor: Red Velvet", "Size: 1lb"

        [MaxLength(60)]
        public string? SKU { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 10000000, ErrorMessage = "Additional price cannot be negative.")]
        public decimal AdditionalPrice { get; set; } = 0.00m;

        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative.")]
        public int StockQuantity { get; set; } = 10;
        public bool IsActive { get; set; } = true;
    }
}
