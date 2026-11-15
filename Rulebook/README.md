# AarMaabd — Engineering Rulebook

**Status:** Baseline standard · **Scope:** Bangladesh single-vendor e-commerce · **Architecture:** ASP.NET Core MVC (C#), Razor, HTML/CSS/JavaScript, Bootstrap 5, EF Core, MySQL-compatible database.

This rulebook is a specification for implementation and review, **not proof that the website is already secure, tested, bug-free, or production-ready**. Every MUST is an acceptance requirement, verified by tests/reviews before release.

## Documents

1. [`ARCHITECTURE.md`](ARCHITECTURE.md) — components, layers, folder structure, dependency direction, request workflows.
2. [`docs/SECURITY_AUTHORIZATION.md`](docs/SECURITY_AUTHORIZATION.md) — authentication, authorization, privacy, payment and file-upload safeguards.
3. [`docs/DATABASE_RULES.md`](docs/DATABASE_RULES.md) — schema, migrations, transactions, consistency and concurrency.
4. [`docs/ERROR_HANDLING_OBSERVABILITY.md`](docs/ERROR_HANDLING_OBSERVABILITY.md) — validation, exception mapping, logs, audit and recovery.
5. [`docs/UI_UX_RULES.md`](docs/UI_UX_RULES.md) — logo-aligned design system, accessibility and shopping journeys.
6. [`docs/PROJECT_RULES.md`](docs/PROJECT_RULES.md) — coding, testing, release gates and non-goals.
7. [`docs/DEPENDENCIES_ENVIRONMENT.md`](docs/DEPENDENCIES_ENVIRONMENT.md) — packages, configuration, version policies, dev/prod setup.
8. [`docs/ACCEPTANCE_CHECKLIST.md`](docs/ACCEPTANCE_CHECKLIST.md) — testable release checklist.

## Non-negotiable project decisions

- **Brand:** AarMaabd; original logo; blush pink, ivory/white, champagne gold, deep rose text. Reference storefront shopping flow: Eloras BD-inspired, **no copied proprietary design/code/assets**.
- **Panels:** Public storefront/guest shopper; customer account; secured Admin Area (`Areas/Admin`). No seller/rider panel in v1.
- **Catalog:** dynamic categories/subcategories/products; crochet, cakes/desserts, savory food, gypsum/candles, resin, accessories, makeup, clothing, bags, gifts.
- **Bangladesh:** BDT (৳); Bangladesh phone/address; configurable delivery zones and charges; cash on delivery; online payment via an approved payment gateway with **server-side verification**.
- **Promotions:** percentage (10%, 20% etc.), fixed amount, category/product/site eligibility; coupons, usage limits, start/end times, stacking policies.
- **Order types:** ready-stock, pre-order/fresh food, customized products; delivery scheduling/lead-time rules.
- **Tech:** ASP.NET Core MVC + Razor Views + C# + EF Core + compatible MySQL/MariaDB provider + Bootstrap + plain JavaScript. No SPA requirement.
- **Privacy:** avoid collecting unneeded personal data; do not store card credentials or plain passwords.
- **Professional quality:** measurable security, performance, concurrency, accessibility, and test acceptance criteria.

## Document precedence and change control

1. Law, gateway agreements and applicable security/privacy requirements;
2. Security and database invariants in this rulebook;
3. Approved product requirements and explicitly agreed scope;
4. UI specifications and implementation details.

If a proposed change conflicts with a higher rule, document the conflict and resolve it before coding. Keep decisions in a change log / architecture decision record (ADR). Every important feature needs: **requirements → design → implementation → tests → review → deployment plan**.

## Delivery phases

- **Phase 1:** working catalog + admin CRUD, Identity accounts/roles, cart, order, COD, delivery calculations, baseline logging/tests.
- **Phase 2:** gateway integration + verified payment, coupon/offer rules, wishlist, custom/pre-orders, email notifications.
- **Phase 3:** analytics, advanced promotions, exports, monitoring, optimization, broader automated tests.

Do not call the product 'production-ready' until the release checklist is completed and live merchant/hosting requirements are met.
