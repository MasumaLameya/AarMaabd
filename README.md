# AarMaabd (আর মায়াবদ) 🌸
> **Handmade With Love & Curated Lifestyle**

An ASP.NET Core MVC E-Commerce web application for handmade resin jewelry, crochet items, custom orders, gifts, and lifestyle collections.

---

## ✨ Features

- **Storefront & Catalog:**
  - Product browsing by category, price filtering, and keyword search.
  - Product details with high-resolution image galleries and variant selection.
- **Custom Order System:**
  - Dedicated custom order request flow with personalized notes, color preferences, and specifications.
- **Cart & Checkout:**
  - Session-based dynamic cart with instant quantity updates.
  - Coupon code discounts and promotional banner integration.
  - Multi-step checkout with delivery address and shipping calculation (Inside/Outside Dhaka).
- **Order Tracking & Invoicing:**
  - Public order tracking using Order Number.
  - Printable invoice generation for customers.
- **Admin Management Portal:**
  - Dashboard overview of sales, active orders, and pending custom requests.
  - CRUD operations for Categories, Products, Offers, Coupons, and Order Statuses.

---

## 🛠️ Tech Stack

- **Backend:** ASP.NET Core 9.0 MVC
- **Database:** MySQL / MariaDB via Pomelo Entity Framework Core
- **Frontend:** Razor Views, Bootstrap 5, FontAwesome, Custom CSS3
- **IDE:** Visual Studio / VS Code

---

## 🚀 Getting Started

### Prerequisites
- [.NET 9.0 SDK](https://dotnet.microsoft.com/)
- MySQL / MariaDB (e.g. XAMPP on Port 3307 or 3306)

### Configuration
Update the connection string in `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Port=3307;Database=aarmaabd_db;User=root;Password=;CharSet=utf8mb4;"
}
```

### Run Locally
```bash
dotnet restore
dotnet build
dotnet run --launch-profile "http"
```
Navigate to `http://localhost:5296` in your browser.
