using AarMaabd.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AarMaabd.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories => Set<Category>();
        public DbSet<SubCategory> SubCategories => Set<SubCategory>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<ProductImage> ProductImages => Set<ProductImage>();
        public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
        public DbSet<CartItem> CartItems => Set<CartItem>();
        public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();
        public DbSet<Offer> Offers => Set<Offer>();
        public DbSet<Coupon> Coupons => Set<Coupon>();
        public DbSet<CouponUsage> CouponUsages => Set<CouponUsage>();
        public DbSet<CustomOrderRequest> CustomOrderRequests => Set<CustomOrderRequest>();
        public DbSet<ProductReview> ProductReviews => Set<ProductReview>();
        public DbSet<Banner> Banners => Set<Banner>();
        public DbSet<StoreSetting> StoreSettings => Set<StoreSetting>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Indexes & Constraints
            builder.Entity<Category>()
                .HasIndex(c => c.Slug)
                .IsUnique();

            builder.Entity<Product>()
                .HasIndex(p => p.Slug)
                .IsUnique();

            builder.Entity<Product>()
                .HasIndex(p => p.SKU)
                .IsUnique();

            builder.Entity<Order>()
                .HasIndex(o => o.OrderNumber)
                .IsUnique();

            builder.Entity<Coupon>()
                .HasIndex(c => c.Code)
                .IsUnique();

            // Safe historical order retention: deleting a product does NOT cascade delete past orders
            builder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<OrderStatusHistory>()
                .HasOne(osh => osh.Order)
                .WithMany(o => o.StatusHistory)
                .HasForeignKey(osh => osh.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed initial store settings if needed
            builder.Entity<StoreSetting>().HasData(
                new StoreSetting { Id = 1, Key = "Store:Announcement", Value = "🌸 Welcome to AarMaabd! Handcrafted with love. Fast delivery inside Dhaka city only!", Category = "Header" },
                new StoreSetting { Id = 2, Key = "Delivery:InsideDhakaFee", Value = "70", Category = "Fulfillment" },
                new StoreSetting { Id = 4, Key = "Delivery:FreeThreshold", Value = "2500", Category = "Fulfillment" },
                new StoreSetting { Id = 5, Key = "Store:SupportPhone", Value = "+880 1700-000000", Category = "Contact" },
                new StoreSetting { Id = 6, Key = "Store:WhatsApp", Value = "+880 1700-000000", Category = "Contact" },
                new StoreSetting { Id = 7, Key = "Store:Address", Value = "Dhanmondi, Dhaka, Bangladesh", Category = "Contact" }
            );
        }
    }
}
