using System.ComponentModel.DataAnnotations;

namespace AarMaabd.Models
{
    public class ProductReview
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        [Required, MaxLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [Range(1, 5)]
        public int Rating { get; set; } = 5;

        [Required, MaxLength(1000)]
        public string Comment { get; set; } = string.Empty;

        public bool IsApproved { get; set; } = true;
        public bool IsVerifiedPurchase { get; set; } = true;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }

    public enum BannerType
    {
        Hero = 1,
        Announcement = 2,
        CollectionTile = 3
    }

    public class Banner
    {
        public int Id { get; set; }

        [Required, MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? Subtitle { get; set; }

        [MaxLength(350)]
        public string ImageUrl { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? TargetUrl { get; set; }

        [MaxLength(50)]
        public string? ButtonText { get; set; }

        public BannerType BannerType { get; set; } = BannerType.Hero;
        public int DisplayOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
    }

    public class StoreSetting
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Key { get; set; } = string.Empty;

        [Required]
        public string Value { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? Description { get; set; }

        [MaxLength(50)]
        public string? Category { get; set; }
    }

    public class AuditLog
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Action { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string EntityName { get; set; } = string.Empty;

        public string? EntityId { get; set; }

        [Required, MaxLength(100)]
        public string PerformedBy { get; set; } = "System";

        public string? Details { get; set; }

        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    }
}
