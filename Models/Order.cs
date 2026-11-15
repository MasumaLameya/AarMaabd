using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AarMaabd.Models
{
    public enum OrderStatus
    {
        Pending = 1,
        Confirmed = 2,
        Preparing = 3,
        Packed = 4,
        Shipped = 5,
        Delivered = 6,
        Cancelled = 7,
        Returned = 8
    }

    public enum PaymentStatus
    {
        Unpaid = 1,
        Pending = 2,
        Paid = 3,
        Failed = 4,
        Refunded = 5
    }

    public enum PaymentMethod
    {
        CashOnDelivery = 1,
        OnlinePayment = 2
    }

    public class Order
    {
        public int Id { get; set; }

        [Required, MaxLength(30)]
        public string OrderNumber { get; set; } = string.Empty;

        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }
        public bool IsGuestOrder { get; set; } = false;

        // Customer contact snapshot
        [Required, MaxLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [Required, MaxLength(25)]
        public string CustomerPhone { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? CustomerEmail { get; set; }

        // Delivery address snapshot
        [Required, MaxLength(50)]
        public string Division { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string District { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string UpazilaThana { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Area { get; set; }

        [Required, MaxLength(250)]
        public string StreetAddress { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? Landmark { get; set; }

        [MaxLength(300)]
        public string? DeliveryNotes { get; set; }

        // Scheduled delivery date for perishable food / custom cakes
        public DateTime? RequestedDeliveryDate { get; set; }

        // Pricing snapshots
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 100000000, ErrorMessage = "Subtotal cannot be negative.")]
        public decimal SubTotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 100000000, ErrorMessage = "Offer discount cannot be negative.")]
        public decimal OfferDiscountAmount { get; set; } = 0.00m;

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 100000000, ErrorMessage = "Coupon discount cannot be negative.")]
        public decimal CouponDiscountAmount { get; set; } = 0.00m;

        [MaxLength(50)]
        public string? AppliedCouponCode { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 100000000, ErrorMessage = "Delivery fee cannot be negative.")]
        public decimal DeliveryFee { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 100000000, ErrorMessage = "Total amount cannot be negative.")]
        public decimal TotalAmount { get; set; }

        // Delivery Zone
        [MaxLength(50)]
        public string DeliveryZone { get; set; } = "Inside Dhaka";

        // Statuses
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CashOnDelivery;

        [MaxLength(100)]
        public string? PaymentTransactionId { get; set; }

        [MaxLength(50)]
        public string? PaymentGatewayName { get; set; }

        // Operational & tracking notes
        [MaxLength(500)]
        public string? AdminNotes { get; set; }

        [MaxLength(300)]
        public string? CancellationReason { get; set; }

        // Timestamps
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? ConfirmedAtUtc { get; set; }
        public DateTime? ShippedAtUtc { get; set; }
        public DateTime? DeliveredAtUtc { get; set; }
        public DateTime? CancelledAtUtc { get; set; }

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();
    }
}
