using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AarMaabd.Models
{
    public enum CustomOrderStatus
    {
        Requested = 1,
        Quoted = 2,
        Approved = 3,
        InProduction = 4,
        Ready = 5,
        Completed = 6,
        Declined = 7
    }

    public class CustomOrderRequest
    {
        public int Id { get; set; }

        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        [Required, MaxLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [Required, MaxLength(25)]
        public string CustomerPhone { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? CustomerEmail { get; set; }

        [Required, MaxLength(50)]
        public string Category { get; set; } = "Crochet"; // Crochet, Cake, Resin, Gypsum, Jewelry, etc.

        [Required, MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? ReferenceImageUrl { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 10000000, ErrorMessage = "Estimated budget cannot be negative.")]
        public decimal? EstimatedBudget { get; set; }

        public DateTime? TargetDeliveryDate { get; set; }

        public CustomOrderStatus Status { get; set; } = CustomOrderStatus.Requested;

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 10000000, ErrorMessage = "Quoted price cannot be negative.")]
        public decimal? QuotedPrice { get; set; }

        [MaxLength(500)]
        public string? AdminNotes { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }
    }
}
