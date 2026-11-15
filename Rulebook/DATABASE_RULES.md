# Database Rules — MySQL / MariaDB + EF Core

## 1. Database and provider policy

- Choose **actual engine** (MySQL 8.x **or** MariaDB bundled with XAMPP) deliberately; do not mix migration assumptions. Use a provider officially compatible with the selected EF Core version.
- Pin matching .NET/EF Core/MySQL provider versions; confirm tested migrations before development. Avoid automatic 'latest' upgrades.
- Use EF Core migrations in version control. Never use `EnsureCreated()` for a migration-managed production database.
- No schema changes by manually editing production database; migration must be reviewed, backed up, staged and reversible where feasible.

## 2. Core model groups

| Domain | Required entities / tables |
|---|---|
| Identity | ASP.NET Identity users, roles, user roles, token tables |
| Catalog | Categories, SubCategories (or Category parent ID), Products, ProductImages, ProductVariants |
| Inventory | InventoryMovements; optional Reservations with expiration |
| Shopping | Carts, CartItems, Wishlists |
| Orders | Orders, OrderItems, OrderStatusHistory, CustomerAddresses / order address snapshot |
| Payments | Payments, PaymentTransactions, Refunds when supported |
| Promotions | Promotions, PromotionTargets, Coupons, CouponRedemptions |
| Content | Banners, Reviews, CustomOrderRequests, ContactMessages, StoreSettings |
| Audit | AuditLogs, only sanitized change metadata |

Use normalized relationships but preserve **immutable purchase snapshots** (item name, SKU/variant, unit price, discounts, delivery info at checkout), so historical orders remain correct when product data changes.

## 3. Schema invariants

1. Every entity has a primary key; FK constraints ensure referential integrity.
2. `SKU`, public `OrderNumber`, gateway transaction IDs and coupon code (as applicable) have unique indexes. Normalize coupon code case consistently.
3. Currency is BDT; use SQL `DECIMAL(18,2)` / C# `decimal` for money, no float/double for prices or monetary totals.
4. Use `NOT NULL`, column lengths, check constraints where supported, correct indexes, defaults explicitly; never rely only on client-side checks.
5. Stock and reserved stock cannot become negative. Quantities must be positive for purchases. Discount cannot exceed eligible amount; order total cannot be negative.
6. Timestamps are stored in UTC; convert to `Asia/Dhaka` for UI. Define start/end offer bounds unambiguously (start inclusive, end exclusive recommended).
7. Keep product IDs durable: do not cascade-delete an item and destroy paid order history. Use archive/`IsActive` or soft-delete where required.
8. Addresses/orders should use order-time snapshots; customer updating saved address must not rewrite old orders.
9. Sensitive personal data and upload references need access policies; do not store raw payment credentials.

## 4. Transaction and concurrency rules

- Order placement must re-fetch prices, stock, offers and delivery server-side **inside a safe transactional workflow**.
- For scarce stock, use atomic conditional SQL update (`UPDATE ... SET Stock = Stock - @q WHERE Id=@id AND Stock >= @q`) and verify affected rows, or equivalent locking/concurrency control. Checking stock in an earlier standalone `SELECT` is insufficient.
- For multiple items, use a consistent locking/order policy, short DB transactions and rollback on failure; account for deadlocks with bounded retries where safe.
- Online payments: define stock reservation record with expiry or clear order-state policy. **Never hold a SQL transaction open while customer completes gateway checkout**.
- Payment callbacks and order submissions are idempotent; unique gateway ID/order idempotency key prevents replay. Payment status changes follow approved transitions.
- Coupon usage limits must be enforced with atomic counters, unique redemption keys and transaction isolation suitable for concurrent checkout. Single-use coupon must not be redeemable twice.
- Customer carts and stock are not authoritative order pricing; recalculate at checkout and clearly report changed price/out-of-stock conflicts.
- `DbContext` is scoped, not singleton, and must not be shared concurrently across tasks.

## 5. Promotion calculation policy (must be documented before launch)

1. Load active product/category/store promotion eligibility; start/end and precedence.
2. Load entered coupon; validate status, user eligibility, min purchase, expiry, total/per-user usage and capped savings.
3. Decide **stacking**: v1 recommended default = **one best eligible automatic offer OR a coupon**, not both, unless marketing explicitly enables stacking.
4. Compute line discounts using deterministic rounding to 2 decimals and record discount breakdown in order snapshot.
5. Apply shipping discounts separately if offer is defined as free shipping.
6. Do not apply expired/used coupons even if a browser shows the old discount.
7. Define cancellation/refund behavior for coupon redemptions so usage is neither leaked nor exploited.

## 6. Query and migration standards

- All growing listings paginate (`Skip/Take` or keyset as appropriate), use appropriate indexes, avoid N+1 query patterns; use `AsNoTracking()` for read-only queries where appropriate.
- Index product category/visibility, order user/date/status, coupon code, transaction gateway ID, stock lookups; verify with real query plans.
- No raw SQL with string interpolation of client input. Parametrize any raw query.
- Migration PR includes generated SQL/review notes for destructive changes; backup before production schema changes.
- Keep seed data safe and environment-specific. Never publish test users' real addresses in demo data.
- Run automated migration smoke test and restore-from-backup rehearsal before public launch.
