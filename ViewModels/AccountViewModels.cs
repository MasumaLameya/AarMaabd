using AarMaabd.Models;
using System.ComponentModel.DataAnnotations;

namespace AarMaabd.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Please enter your email or phone number")]
        [Display(Name = "Email or Mobile Number")]
        public string LoginIdentifier { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your password")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; } = true;

        public string? ReturnUrl { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Full Name is required")]
        [Display(Name = "Full Name")]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mobile number is required")]
        [RegularExpression(@"^(?:\+?88)?01[3-9]\d{8}$", ErrorMessage = "Enter valid Bangladeshi phone (e.g. 01712345678)")]
        [Display(Name = "Mobile Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Display(Name = "Division")]
        public string? Division { get; set; } = "Dhaka";

        [Display(Name = "District")]
        public string? District { get; set; } = "Dhaka";

        [Display(Name = "Upazila / Thana")]
        public string? UpazilaThana { get; set; }

        [Display(Name = "Street Address")]
        public string? StreetAddress { get; set; }

        public string? ReturnUrl { get; set; }
    }

    public class CustomerDashboardViewModel
    {
        public ApplicationUser User { get; set; } = null!;
        public List<Order> RecentOrders { get; set; } = new();
        public List<WishlistItem> Wishlist { get; set; } = new();
        public List<CustomOrderRequest> CustomOrders { get; set; } = new();
        public int TotalOrdersCount { get; set; }
        public decimal TotalSpent { get; set; }
    }

    public class OrderTrackingRequestViewModel
    {
        [Required(ErrorMessage = "Order Number is required (e.g. AAR-20261021-1234)")]
        [Display(Name = "Order Number")]
        public string OrderNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number used during checkout is required")]
        [Display(Name = "Phone Number")]
        public string CustomerPhone { get; set; } = string.Empty;
    }
}
