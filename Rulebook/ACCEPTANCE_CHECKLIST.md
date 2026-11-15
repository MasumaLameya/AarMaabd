# AarMaabd Release Acceptance Checklist

Use this list at each release; **unchecked items block public launch when applicable**.

## Security & Identity

- [ ] Admin area denies guests and Customers on every endpoint (not just hidden menus).
- [ ] Customer cannot access another customer's orders, addresses, invoice or custom request.
- [ ] Guest order tracking verifies ownership beyond a guessable order number.
- [ ] Passwords are Identity hashes, login is rate-limited, reset flow uses expiring tokens.
- [ ] Anti-forgery validation and input validation on every state-changing form.
- [ ] Upload rejects invalid, oversized and executable files; private image access restricted.
- [ ] HTTPS and secure cookies; no credentials/tokens/secrets committed or exposed.

## Catalog / Pricing / Orders

- [ ] Dynamic categories and subcategories work without code changes.
- [ ] Product visibility, images, variants, stock, search, filter, pagination work.
- [ ] 10%/20% or fixed discount displays and calculates accurately.
- [ ] Coupon min spend, expiry, caps, usage limits and stacking rules verified.
- [ ] Shipping charge and BDT totals calculated server-side.
- [ ] COD creates correct order/payment state; receipt/order reference shown.
- [ ] Checkout duplicate submission does not create duplicate order.
- [ ] Two simultaneous buyers of last item cannot oversell.
- [ ] Two simultaneous coupon redeemers cannot bypass cap.
- [ ] Historic order details remain unchanged after product price/edit/archive.

## Payments

- [ ] Gateway hosted flow is sandbox-tested (before live launch).
- [ ] Browser redirect alone never changes payment status to Paid.
- [ ] Server verifies gateway transaction identity, amount, currency and result.
- [ ] Replayed callback does not duplicate credit/stock/orders.
- [ ] Failed/expired/cancelled payment releases reservations correctly.
- [ ] Refund/manual status changes are authorized and audited.

## UI, Logs, Reliability

- [ ] Logo brand palette consistent on desktop/tablet/mobile.
- [ ] Accessible navigation, meaningful alt text, visible focus and readable contrast.
- [ ] Checkout forms are clear for Bangladesh address/phone/delivery.
- [ ] Errors show helpful safe messages; raw stack traces never public.
- [ ] Application logs redact PII/secrets and correlate failures.
- [ ] Migrations tested; backups, restore and rollback procedure verified.
- [ ] Smoke, integration, authorization, payment and concurrency tests pass.
- [ ] Load test on staging measured and known limitations documented.
- [ ] Operational ownership/contact and incident-response process agreed.
