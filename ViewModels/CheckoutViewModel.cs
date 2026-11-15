using AarMaabd.Models;
using System.ComponentModel.DataAnnotations;

namespace AarMaabd.ViewModels
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Please enter your full name")]
        [Display(Name = "Full Name")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your Bangladeshi mobile number")]
        [RegularExpression(@"^(?:\+?88)?01[3-9]\d{8}$", ErrorMessage = "Please enter a valid Bangladeshi mobile number (e.g. 01712345678)")]
        [Display(Name = "Mobile Number")]
        public string CustomerPhone { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [Display(Name = "Email Address (Optional)")]
        public string? CustomerEmail { get; set; }

        [Required(ErrorMessage = "Please select your Division")]
        [Display(Name = "Division")]
        public string Division { get; set; } = "Dhaka";

        [Required(ErrorMessage = "Please enter your District")]
        [Display(Name = "District")]
        public string District { get; set; } = "Dhaka";

        [Required(ErrorMessage = "Please enter your Upazila / Thana")]
        [Display(Name = "Upazila / Thana")]
        public string UpazilaThana { get; set; } = string.Empty;

        [Display(Name = "Area / Neighborhood")]
        public string? Area { get; set; }

        [Required(ErrorMessage = "Please enter your detailed street address")]
        [Display(Name = "Street Address / House & Road")]
        public string StreetAddress { get; set; } = string.Empty;

        [Display(Name = "Nearby Landmark (Optional)")]
        public string? Landmark { get; set; }

        [Display(Name = "Special Delivery Notes (Optional)")]
        public string? DeliveryNotes { get; set; }

        [Display(Name = "Preferred Delivery Date (For Cakes & Perishables)")]
        [DataType(DataType.Date)]
        public DateTime? RequestedDeliveryDate { get; set; }

        [Required]
        [Display(Name = "Delivery Zone")]
        public string DeliveryZone { get; set; } = "Inside Dhaka"; // Strictly Inside Dhaka

        [Required]
        [Display(Name = "Payment Method")]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CashOnDelivery;

        public CartSummaryViewModel CartSummary { get; set; } = new();

        public string? AppliedCouponCode { get; set; }
    }

    public class OrderConfirmationViewModel
    {
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string DeliveryAddress { get; set; } = string.Empty;
        public string DeliveryZone { get; set; } = string.Empty;
        public DateTime? RequestedDeliveryDate { get; set; }
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public List<OrderItemViewModel> Items { get; set; } = new();
    }

    public class OrderItemViewModel
    {
        public string ProductName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? Sku { get; set; }
        public string? VariantName { get; set; }
        public string? CustomizationNote { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal => UnitPrice * Quantity;
    }
}
