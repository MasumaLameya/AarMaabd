# UI/UX and Frontend Design Rules

## Brand and reference

AarMaabd uses its **own** logo and identity. Inspired by the shopping structure of `https://www.elorasbd.com/` (promotional hero, browsable categories, product grid/details, cart, checkout), **not an asset/code copy**. Preserve the logo artwork unless owner requests an edit.

### Base design tokens (starting values, adjust after checking original logo on screen)

```css
:root {
  --brand-blush: #F8DDE4;
  --brand-ivory: #FFF9F7;
  --brand-gold: #C9A46A;
  --brand-rose: #753D51;
  --text-primary: #363036;
  --border-soft: #E8D3D9;
  --surface: #FFFFFF;
  --radius-card: 14px;
  --space-unit: 4px;
}
```

- White/ivory base; blush used for promotional/emphasis sections; gold used sparingly as decoration/accents; dark rose/charcoal for accessible text.
- Use consistent typography: elegant restrained serif for main brand/section headlines, readable sans-serif for body/forms/prices. Keep sizes and line heights consistent.
- Keep sufficient contrast for body text, buttons, labels and focus indicators; decorative pale gold must not be relied upon for essential text.
- Avoid excessive animations and autoplay distractions; honor `prefers-reduced-motion`.
- Use standardized button, input, badge, card, empty-state, alert, modal and loading styles across both storefront and dashboard.

## Public site / homepage structure

1. Announcement bar: delivery / promotion message from Admin CMS.
2. Header: original logo, search, account, wishlist, cart badge; mobile menu; sticky only if it doesn't block content.
3. Category navigation: 10 dynamic categories; responsive dropdown for subcategories. No hardcoded menu names.
4. Hero banner/slider: accessible manual controls, valid alt text, optional promotion link; maintain readable overlay.
5. Shop by Category: image + name, responsive cards.
6. Featured Products, Best Sellers, New Arrivals: dynamic product cards.
7. Special Offers: 10%/20% promotion badges + true discounted price, start/end validity.
8. Ready-stock/Cakes & Desserts/Savory Foods/Custom Gifts callouts where relevant.
9. Trust/help: delivery, COD/payment options, contact, authentic policies.
10. Reviews/testimonials only if real, moderated and allowed; Footer with policies/contact/social links.

## Product / shop pages

- Product card: optimized image, title, current price, struck-through regular price **only when valid discount**, % badge, stock label, wishlist, Add to Cart. Use BDT `৳` and clear currency formatting.
- Detail page: image gallery; title/SKU, variants (size/color/flavor), stock or pre-order lead time, ingredients/allergens for food, care instructions where relevant, quantity, delivery availability, Add to Cart/Buy Now, reviews, related products.
- Filter/sort/search: category, price, availability, latest and price ordering; clear filters, pagination and empty-result handling.
- Use responsive product grids; prevent layout shift by assigning image aspect ratios and sizes; compress/lazy-load below fold.

## Cart & checkout UX

- Keep cart editable with remove/quantity, subtotal, discount, shipping, total clearly itemized.
- Recalculate server-side; if prices/stock changed, show a clear, non-alarming notice before final confirmation.
- Support guest checkout if enabled, plus optional account registration; do not force unnecessary data collection.
- Bangladesh form: name, phone, Division, District, Upazila/Thana, area, detailed address, landmark optional, delivery notes; preserve choice on validation failure.
- Delivery zone, estimated fee and available slot (food/cake/custom pre-order) must be shown before placing order.
- COD and gateway-supported online payment are clearly separated; never ask users to share wallet PIN/OTP with AarMaabd staff.
- Confirm order only when saved correctly. Show order number, payment state, delivery summary and next step. Never call unverified online payment 'Paid'.
- Offer/coupon input: code, validation feedback, discount amount, eligibility and clear stacking rule; accessible error near input.

## Admin UX

- Admin visual language matches brand lightly, but favors functional tables, clear actions, filters, search and status badges.
- Every destructive action asks confirmation and has a safe archive/deactivate alternative; prevent accidental duplicate submissions.
- CRUD forms show required fields, preview uploaded images, errors near fields, save/cancel feedback and optimistic conflict messages.
- Present order status and payment status **separately**. Important actions (cancel, refund, payment override) require confirmation, authorization and audit.
- Dashboard stats must be real metrics with date range labels; mock values only in prototypes and must be labelled.

## Accessibility, responsiveness, SEO

- Meet WCAG 2.2 AA targets where feasible: keyboard navigation, visible focus, semantic headings/forms, alt text, color contrast, descriptive links, error identification.
- Minimum test widths: 320/375px phones, tablet (~768px), laptop (~1024px), desktop (~1440px); test touch and keyboard.
- Use unique page titles/meta descriptions, structured semantic HTML, human-readable product slugs, canonical strategy and sitemap as appropriate.
- Real product images need rights/permission. Never display false review counts or fake discount pricing.
