using AarMaabd.Models;

namespace AarMaabd.ViewModels
{
    public class ShopViewModel
    {
        public List<Category> Categories { get; set; } = new();
        public List<Product> Products { get; set; } = new();

        public int? SelectedCategoryId { get; set; }
        public int? SelectedSubCategoryId { get; set; }
        public string? SelectedCategorySlug { get; set; }
        public string? SelectedCategoryName { get; set; }

        public string? SearchQuery { get; set; }
        public string? SortBy { get; set; } = "featured"; // featured, latest, price_asc, price_desc, rating
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool? InStockOnly { get; set; }

        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public int TotalProducts { get; set; } = 0;
    }

    public class ProductDetailsViewModel
    {
        public Product Product { get; set; } = null!;
        public List<Product> RelatedProducts { get; set; } = new();
        public List<ProductReview> Reviews { get; set; } = new();
        public double AverageRating { get; set; } = 5.0;
        public int TotalReviewsCount { get; set; } = 0;

        // Customization & Ordering options
        public int Quantity { get; set; } = 1;
        public int? SelectedVariantId { get; set; }
        public string? CustomizationNote { get; set; }
    }

    public class HomeViewModel
    {
        public List<Banner> HeroBanners { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public List<Product> FeaturedProducts { get; set; } = new();
        public List<Product> BestSellers { get; set; } = new();
        public List<Product> NewArrivals { get; set; } = new();
        public List<Offer> ActiveOffers { get; set; } = new();
        public string AnnouncementText { get; set; } = string.Empty;
    }
}
