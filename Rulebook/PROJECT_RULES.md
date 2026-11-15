# Full Project Engineering Rules

## Mandatory coding standards

1. Use C# naming: PascalCase classes/methods, camelCase local variables, `Async` suffix for async methods, predictable feature naming.
2. Controllers are thin: authenticate/authorize, validate request, call service, return View/Redirect/Result. Order/payment calculations belong in services.
3. Never write SQL in `.cshtml`; no business logic in views; views use purpose-built ViewModels.
4. Use async database/network I/O; propagate `CancellationToken` where practical, not `async void`.
5. Do not share scoped services/DbContexts across requests or concurrent tasks, or store mutable customer data in `static` fields.
6. Use typed configuration/options, DI via constructor, and consistent logging; keep secrets outside git.
7. Protect all POST/PUT/PATCH/DELETE endpoints by server-side validation, authentication/ownership, antiforgery as applicable, and explicit idempotency for business-critical submits.
8. Use decimal for currency, UTC for timestamps, safe identifiers for uploads, database constraints for invariants.
9. Use archive/deactivate for referenced products/categories; never break historical orders.
10. All text/UI (buttons, validation errors, order status) must be readable and consistent, not only visually aesthetic.

## Feature rules

| Feature | Hard rule |
|---|---|
| Dynamic catalog | Category/subcategory/product visibility and ordering from DB; no per-category controller required |
| Pricing | Total derived server-side; order snapshots never change if catalog price updates |
| Offers | Product/category/store targeting; time windows; explicit precedence and stacking policy |
| Coupon | Eligibility, min spend, max discount, usage caps and concurrency-safe redemption |
| Inventory | Atomic stock checks + rollback; reserve/release on failed payment where applicable |
| Orders | Explicit finite state transitions; idempotent submission; audit status changes |
| Payment | Hosted gateway, signed/verified callbacks, unique transactions; never mark paid by redirect alone |
| Custom cake/food | Lead time, safe delivery scheduling and product-specific disclosures |
| Uploads | Strict image validation; access control for customer reference images |
| Admin | Explicit role and operation authorization; important updates auditable |
| Guest checkout | Only if secure guest ownership/tracking verification is built |

## Quality gates per feature

Each pull request / commit series SHOULD include: requirement/reference, screenshots where UI changed, migration if schema changed, validation cases, negative cases, authorization checks, tests, and manual verification steps.

**Definition of Done:** feature works end-to-end on clean local database, error paths behave correctly, proper mobile layout, input validation, access rules, no secrets committed, no critical test failures. Production release requires additional deployment checklist.

## Testing matrix

- Unit: price/discount rounding, shipping, coupon conditions, stock status transitions, order state machine.
- Integration: Identity roles, EF migrations/relations, persistence, checkout, gateway adapter mock, restore/rollback.
- Security: role bypass, cross-user object access, CSRF, XSS, malicious upload, login throttle, leaked secrets.
- Concurrency: two buyers for last unit, duplicated checkout requests, coupon last redemption, gateway callback replay.
- UI/E2E: navbar, category/subcategory, product details, cart, discount/coupon, address, COD, gateway sandbox, admin order updates.
- Performance: targeted concurrent-user load tests against staging; measure p95 response, throughput, 5xx errors, database load. No unsupported concurrent-user capacity promises.

## Branch/deployment rules

- Keep `main` deployable; implement changes in small feature branches; code reviews (even self-review checklist) before merge.
- Pin dependencies, check vulnerabilities and maintain updates; commit migration files with entity changes.
- Have dev and production configs/secrets isolated; do not use live gateway keys in development.
- Deploy to staging first; run smoke test, verify migration and rollback; then production backup and deploy.
- Document known limitations clearly. Never promise '100% bug-free'.

## In-scope vs not-in-scope (v1)

**In v1:** public/catalog, customer/guest where enabled, secure Admin Area, product/category CRUD, stock, cart, COD order flow, delivery charges, basic coupons/offers if implemented with full validation; brand matching responsive UI.

**Later / conditional:** live payment gateway after merchant approval, custom order quotations, enhanced reports, newsletter, SMS, customer reviews, automated refund integration.

**Not required for v1:** multi-vendor marketplace, rider GPS tracking, separate native mobile app, AI chatbot, React/Angular/Next.js SPA, Kubernetes, microservices, blockchain, complicated loyalty points. Add only after a real requirement and feasibility review.
