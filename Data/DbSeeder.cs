using AarMaabd.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AarMaabd.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Ensure database is created and migrations applied
            await context.Database.EnsureCreatedAsync();

            // 1. Seed Roles
            string[] roles = { "Admin", "Customer" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. Seed Default Admin User
            var adminEmail = "admin@aarmaabd.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "AarMaabd Admin",
                    PhoneNumber = "+8801700000000",
                    EmailConfirmed = true,
                    Division = "Dhaka",
                    District = "Dhaka",
                    UpazilaThana = "Dhanmondi",
                    Area = "Dhanmondi 27",
                    StreetAddress = "House 12, Road 4",
                    CreatedAtUtc = DateTime.UtcNow
                };
                var result = await userManager.CreateAsync(adminUser, "Admin@123456");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // 3. Seed Demo Customer User
            var customerEmail = "customer@aarmaabd.com";
            var customerUser = await userManager.FindByEmailAsync(customerEmail);
            if (customerUser == null)
            {
                customerUser = new ApplicationUser
                {
                    UserName = customerEmail,
                    Email = customerEmail,
                    FullName = "Nafisa Rahman",
                    PhoneNumber = "01812345678",
                    EmailConfirmed = true,
                    Division = "Dhaka",
                    District = "Dhaka",
                    UpazilaThana = "Gulshan",
                    Area = "Gulshan 1",
                    StreetAddress = "Avenue 3, Plot 18",
                    CreatedAtUtc = DateTime.UtcNow
                };
                var result = await userManager.CreateAsync(customerUser, "Customer@123456");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(customerUser, "Customer");
                }
            }

            // 4. Seed Categories
            if (!await context.Categories.AnyAsync())
            {
                var categories = new List<Category>
                {
                    new() { Name = "Crochet", Slug = "crochet", Description = "Handmade crochet bags, clothes, keyrings and delicate home accessories.", ImageUrl = "/images/products/crochet-bag.jpg", IconClass = "bi-bag-heart", DisplayOrder = 1 },
                    new() { Name = "Cakes & Desserts", Slug = "cakes-desserts", Description = "Freshly baked artisan bento cakes, cookies, pancakes, cupcakes and jar cakes.", ImageUrl = "/images/products/pink-cake.jpg", IconClass = "bi-cake2", DisplayOrder = 2 },
                    new() { Name = "Savory Food", Slug = "savory-food", Description = "Delicious handmade sandwiches, creamy pasta, spicy chowmein and fried rice.", ImageUrl = "/images/branding/hero.jpg", IconClass = "bi-egg-fried", DisplayOrder = 3 },
                    new() { Name = "Gypsum & Candles", Slug = "gypsum-candles", Description = "Aesthetic gypsum ribbed trays, scented bubble candles and minimalist home decor.", ImageUrl = "/images/branding/logo.jpg", IconClass = "bi-lamp", DisplayOrder = 4 },
                    new() { Name = "Resin Art", Slug = "resin-art", Description = "Handcrafted crystal epoxy keychains, floral pendants, bangles and bookmarks.", ImageUrl = "/images/products/resin-keychain.jpg", IconClass = "bi-gem", DisplayOrder = 5 },
                    new() { Name = "Jewellery & Accessories", Slug = "jewellery-accessories", Description = "Traditional churi, bangles, pearl pendants, necklaces and hair accessories.", ImageUrl = "/images/branding/hero.jpg", IconClass = "bi-stars", DisplayOrder = 6 },
                    new() { Name = "Makeup & Beauty", Slug = "makeup-beauty", Description = "Curated lip tints, organic lip oils, blushers and beauty essentials.", ImageUrl = "/images/branding/logo.jpg", IconClass = "bi-brush", DisplayOrder = 7 },
                    new() { Name = "Clothes & Wear", Slug = "clothes-wear", Description = "Custom knitted cardigans, hand-embroidered clothing and delicate scarves.", ImageUrl = "/images/branding/hero.jpg", IconClass = "bi-person-standing-dress", DisplayOrder = 8 },
                    new() { Name = "Bags & Purses", Slug = "bags-purses", Description = "Artisanal crochet tote bags, chic sling bags and clutches.", ImageUrl = "/images/products/crochet-bag.jpg", IconClass = "bi-handbag", DisplayOrder = 9 },
                    new() { Name = "Gifts & Combos", Slug = "gifts-combos", Description = "Specially wrapped gift hampers, birthday boxes and celebration combos.", ImageUrl = "/images/products/resin-keychain.jpg", IconClass = "bi-gift", DisplayOrder = 10 }
                };

                await context.Categories.AddRangeAsync(categories);
                await context.SaveChangesAsync();

                // Subcategories
                var crochetCat = categories.First(c => c.Slug == "crochet");
                var foodCat = categories.First(c => c.Slug == "cakes-desserts");
                var resinCat = categories.First(c => c.Slug == "resin-art");

                var subCategories = new List<SubCategory>
                {
                    new() { CategoryId = crochetCat.Id, Name = "Crochet Bags", Slug = "crochet-bags", DisplayOrder = 1 },
                    new() { CategoryId = crochetCat.Id, Name = "Keyrings & Charms", Slug = "crochet-keyrings", DisplayOrder = 2 },
                    new() { CategoryId = crochetCat.Id, Name = "Wearable Clothes", Slug = "crochet-clothes", DisplayOrder = 3 },
                    new() { CategoryId = foodCat.Id, Name = "Bento & Layer Cakes", Slug = "bento-cakes", DisplayOrder = 1 },
                    new() { CategoryId = foodCat.Id, Name = "Artisan Cupcakes", Slug = "cupcakes", DisplayOrder = 2 },
                    new() { CategoryId = foodCat.Id, Name = "Jar Cakes & Sweets", Slug = "jar-cakes", DisplayOrder = 3 },
                    new() { CategoryId = resinCat.Id, Name = "Letter Keychains", Slug = "resin-keychains", DisplayOrder = 1 },
                    new() { CategoryId = resinCat.Id, Name = "Pendants & Jewelry", Slug = "resin-jewelry", DisplayOrder = 2 }
                };
                await context.SubCategories.AddRangeAsync(subCategories);
                await context.SaveChangesAsync();

                // 5. Seed Products
                var subCrochetBags = subCategories.First(s => s.Slug == "crochet-bags");
                var subCupcakes = subCategories.First(s => s.Slug == "cupcakes");
                var subBento = subCategories.First(s => s.Slug == "bento-cakes");
                var subResin = subCategories.First(s => s.Slug == "resin-keychains");

                var products = new List<Product>
                {
                    new()
                    {
                        CategoryId = crochetCat.Id,
                        SubCategoryId = subCrochetBags.Id,
                        Name = "Floral Pastel Crochet Handbag",
                        Slug = "floral-pastel-crochet-handbag",
                        SKU = "CR-BAG-001",
                        ShortDescription = "Handmade pastel knit bag with 3D floral roses and delicate pearl accents.",
                        Description = "Lovingly hand-knitted with ultra-soft organic cotton yarn. Features hand-crocheted pastel blossoms, daisies, satin ribbon bows, and pearl embellishments. Sturdy handles with reinforced lining inside.",
                        Price = 1150.00m,
                        SalePrice = 950.00m,
                        StockQuantity = 15,
                        IsActive = true,
                        IsFeatured = true,
                        IsBestSeller = true,
                        ProductType = ProductType.ReadyStock,
                        AvailableColors = "Cream White, Pastel Pink, Baby Blue",
                        CareInstructions = "Gently hand wash in cold water with mild detergent; flat dry.",
                        Images = new List<ProductImage>
                        {
                            new() { ImageUrl = "/images/products/crochet-bag.jpg", AltText = "Floral Pastel Crochet Handbag", IsPrimary = true, DisplayOrder = 1 }
                        }
                    },
                    new()
                    {
                        CategoryId = foodCat.Id,
                        SubCategoryId = subCupcakes.Id,
                        Name = "Signature Red Velvet Cupcake",
                        Slug = "signature-red-velvet-cupcake",
                        SKU = "FD-CUP-001",
                        ShortDescription = "Melt-in-mouth red velvet cupcake topped with silky cream cheese swirl and ruby crumbs.",
                        Description = "Moist, velvety cocoa crumb infused with Madagascar vanilla bean, crowned with luscious New York cream cheese swirl frosting. Freshly prepared on order in hygienic artisan kitchen.",
                        Price = 220.00m,
                        SalePrice = 180.00m,
                        StockQuantity = 25,
                        IsActive = true,
                        IsFeatured = true,
                        IsNewArrival = true,
                        ProductType = ProductType.FoodPerishable,
                        PreparationLeadDays = 1,
                        Ingredients = "Flour, Cocoa, Sugar, Butter, Cream Cheese, Buttermilk, Vanilla",
                        AvailableFlavors = "Classic Red Velvet, Belgian Chocolate, Salted Caramel",
                        Images = new List<ProductImage>
                        {
                            new() { ImageUrl = "/images/products/velvet-cupcake.jpg", AltText = "Signature Red Velvet Cupcake", IsPrimary = true, DisplayOrder = 1 }
                        }
                    },
                    new()
                    {
                        CategoryId = foodCat.Id,
                        SubCategoryId = subBento.Id,
                        Name = "Strawberry Korean Bento Cake",
                        Slug = "strawberry-korean-bento-cake",
                        SKU = "FD-CAKE-001",
                        ShortDescription = "Aesthetic 4-inch pastel pink strawberry buttercream bento cake topped with fresh berries.",
                        Description = "Tender Japanese sponge cake layered with fresh strawberry compote and whipped vanilla cream, coated with aesthetic pastel pink buttercream. Perfect for birthdays and anniversaries.",
                        Price = 850.00m,
                        SalePrice = 750.00m,
                        StockQuantity = 10,
                        IsActive = true,
                        IsFeatured = true,
                        ProductType = ProductType.FoodPerishable,
                        PreparationLeadDays = 1,
                        AvailableSizes = "4 inch (Bento), 6 inch (1 Pound), 8 inch (2 Pound)",
                        AvailableFlavors = "Strawberry Cream, Vanilla Bean, Chocolate Fudge",
                        Ingredients = "Egg, Flour, Strawberries, Dairy Cream, Butter, Sugar",
                        Images = new List<ProductImage>
                        {
                            new() { ImageUrl = "/images/products/pink-cake.jpg", AltText = "Strawberry Korean Bento Cake", IsPrimary = true, DisplayOrder = 1 }
                        }
                    },
                    new()
                    {
                        CategoryId = resinCat.Id,
                        SubCategoryId = subResin.Id,
                        Name = "Botanical Rose & Gold Foil Resin Keychain",
                        Slug = "botanical-rose-gold-resin-keychain",
                        SKU = "RS-KEY-001",
                        ShortDescription = "Custom initial alphabet keychain with real dried rose petals, 24k gold leaf flakes, and freshwater pearl.",
                        Description = "Hand-poured crystal clear epoxy resin encasing real dried botanical pink petals and gleaming gold foil flakes. Comes with a gold-tone brass keyring and freshwater pearl charm. Custom letter available from A to Z.",
                        Price = 450.00m,
                        SalePrice = 380.00m,
                        StockQuantity = 30,
                        IsActive = true,
                        IsFeatured = true,
                        IsBestSeller = true,
                        ProductType = ProductType.ReadyStock,
                        AvailableSizes = "Letter A to Z",
                        CareInstructions = "Wipe with a soft cloth; avoid prolonged exposure to direct extreme heat.",
                        Images = new List<ProductImage>
                        {
                            new() { ImageUrl = "/images/products/resin-keychain.jpg", AltText = "Botanical Rose & Gold Foil Resin Keychain", IsPrimary = true, DisplayOrder = 1 }
                        }
                    }
                };

                await context.Products.AddRangeAsync(products);
                await context.SaveChangesAsync();
            }

            // 6. Seed Promotional Offers (10% & 20%)
            if (!await context.Offers.AnyAsync())
            {
                var offers = new List<Offer>
                {
                    new()
                    {
                        Title = "Resin & Crochet Fest — 10% OFF",
                        BadgeText = "10% OFF",
                        DiscountType = DiscountType.Percentage,
                        DiscountValue = 10.00m,
                        TargetType = OfferTargetType.AllProducts,
                        StartDateUtc = DateTime.UtcNow.AddDays(-1),
                        EndDateUtc = DateTime.UtcNow.AddMonths(2),
                        IsActive = true,
                        DisplayOrder = 1
                    },
                    new()
                    {
                        Title = "Celebration Specials — 20% OFF",
                        BadgeText = "20% OFF",
                        DiscountType = DiscountType.Percentage,
                        DiscountValue = 20.00m,
                        TargetType = OfferTargetType.AllProducts,
                        StartDateUtc = DateTime.UtcNow.AddDays(-1),
                        EndDateUtc = DateTime.UtcNow.AddMonths(2),
                        IsActive = true,
                        DisplayOrder = 2
                    }
                };
                await context.Offers.AddRangeAsync(offers);
                await context.SaveChangesAsync();
            }

            // 7. Seed Coupons
            if (!await context.Coupons.AnyAsync())
            {
                var coupons = new List<Coupon>
                {
                    new()
                    {
                        Code = "AAR10",
                        Description = "Get 10% discount on orders above ৳500",
                        DiscountType = DiscountType.Percentage,
                        DiscountValue = 10.00m,
                        MinSpendAmount = 500.00m,
                        MaxDiscountCap = 300.00m,
                        UsageLimitTotal = 500,
                        UsageLimitPerCustomer = 2,
                        StartDateUtc = DateTime.UtcNow.AddDays(-1),
                        EndDateUtc = DateTime.UtcNow.AddMonths(6),
                        IsActive = true
                    },
                    new()
                    {
                        Code = "WELCOME20",
                        Description = "Special 20% discount on first orders above ৳1,000",
                        DiscountType = DiscountType.Percentage,
                        DiscountValue = 20.00m,
                        MinSpendAmount = 1000.00m,
                        MaxDiscountCap = 500.00m,
                        UsageLimitTotal = 200,
                        UsageLimitPerCustomer = 1,
                        FirstOrderOnly = true,
                        StartDateUtc = DateTime.UtcNow.AddDays(-1),
                        EndDateUtc = DateTime.UtcNow.AddMonths(6),
                        IsActive = true
                    },
                    new()
                    {
                        Code = "FLAT100",
                        Description = "Save ৳100 instantly on orders over ৳800",
                        DiscountType = DiscountType.FixedAmount,
                        DiscountValue = 100.00m,
                        MinSpendAmount = 800.00m,
                        UsageLimitTotal = 300,
                        UsageLimitPerCustomer = 1,
                        StartDateUtc = DateTime.UtcNow.AddDays(-1),
                        EndDateUtc = DateTime.UtcNow.AddMonths(6),
                        IsActive = true
                    }
                };
                await context.Coupons.AddRangeAsync(coupons);
                await context.SaveChangesAsync();
            }

            // 8. Seed Banners
            if (!await context.Banners.AnyAsync())
            {
                var banners = new List<Banner>
                {
                    new()
                    {
                        Title = "Handmade With Love",
                        Subtitle = "Exquisite crochet bags, delicate resin jewelry, and delicious artisan cakes.",
                        ImageUrl = "/images/branding/hero.jpg",
                        TargetUrl = "/Shop",
                        ButtonText = "Explore Collection",
                        BannerType = BannerType.Hero,
                        DisplayOrder = 1,
                        IsActive = true
                    }
                };
                await context.Banners.AddRangeAsync(banners);
                await context.SaveChangesAsync();
            }
        }
    }
}
