using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AarMaabd.Models
{
    public enum ProductType
    {
        ReadyStock = 1,
        PreOrder = 2,
        CustomMade = 3,
        FoodPerishable = 4
    }

    public class Product
    {
        public int Id { get; set; }

        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public int? SubCategoryId { get; set; }
        public SubCategory? SubCategory { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(220)]
        public string Slug { get; set; } = string.Empty;

        [Required, MaxLength(60)]
        public string SKU { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? ShortDescription { get; set; }

        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 10000000, ErrorMessage = "Regular price cannot be negative.")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 10000000, ErrorMessage = "Sale price cannot be negative.")]
        public decimal? SalePrice { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative.")]
        public int StockQuantity { get; set; } = 10;

        [Range(0, int.MaxValue, ErrorMessage = "Low stock threshold cannot be negative.")]
        public int LowStockThreshold { get; set; } = 3;

        public bool IsActive { get; set; } = true;
        public bool IsFeatured { get; set; } = false;
        public bool IsNewArrival { get; set; } = false;
        public bool IsBestSeller { get; set; } = false;

        public ProductType ProductType { get; set; } = ProductType.ReadyStock;

        [Range(0, 365, ErrorMessage = "Preparation days cannot be negative.")]
        public int PreparationLeadDays { get; set; } = 0;

        [MaxLength(255)]
        public string? Ingredients { get; set; }

        [MaxLength(255)]
        public string? CareInstructions { get; set; }

        [MaxLength(150)]
        public string? AvailableSizes { get; set; }

        [MaxLength(150)]
        public string? AvailableColors { get; set; }

        [MaxLength(150)]
        public string? AvailableFlavors { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }

        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
        public ICollection<ProductReview> Reviews { get; set; } = new List<ProductReview>();

        [NotMapped]
        public decimal CurrentEffectivePrice => SalePrice.HasValue && SalePrice.Value > 0 && SalePrice.Value < Price ? SalePrice.Value : Price;

        [NotMapped]
        public bool HasDiscount => SalePrice.HasValue && SalePrice.Value > 0 && SalePrice.Value < Price;

        [NotMapped]
        public int DiscountPercentage => HasDiscount ? (int)Math.Round(((Price - SalePrice!.Value) / Price) * 100) : 0;

        [NotMapped]
        public string PrimaryImageUrl => Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl 
            ?? Images.FirstOrDefault()?.ImageUrl 
            ?? "/images/branding/logo.jpg";
    }
}
