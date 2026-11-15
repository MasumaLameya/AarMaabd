# Security, Authentication & Authorization Rules

**Baseline:** Follow current OWASP ASVS / Cheat Sheets and Microsoft ASP.NET Core security guidance. Security MUST be validated in development, staging and production.

## Identity and authentication

- MUST use **ASP.NET Core Identity** (or equivalent mature password storage) and store only salted password hashes. Never store plain, reversible encrypted or manually SHA256-only passwords.
- MUST use HTTPS in production; secure, HttpOnly authentication cookies, deliberate SameSite policy, HSTS with proper hosting, and protected Data Protection keys.
- MUST enforce server-side password policy, unique normalized login identifier, token-based verified password reset, and email/phone verification where appropriate.
- MUST rate-limit login, password reset, registration and sensitive endpoints; configure appropriate lockout and user-enumeration-resistant messages.
- MUST rotate/invalidate sessions appropriately after password reset or security-sensitive account changes; enforce anti-forgery tokens on cookie-authenticated state-changing requests.
- SHOULD enable MFA for Admin users before public launch. Initial admin creation must use a controlled setup/seed flow, not a hardcoded public credential.

## Roles, resource ownership, authorization matrix

| Action | Guest | Customer | Admin |
|---|---:|---:|---:|
| View public products/categories | Allow | Allow | Allow |
| Use guest cart | Allow | Allow | Allow |
| Guest checkout (if enabled) | Restricted, verified | Allow | Allow |
| View own orders/address | Verified ownership mechanism | Own only | Authorized support access only |
| Edit own profile | No | Own only | Own only / audited support mechanism |
| Leave verified-purchase review | No | Own eligible orders | Moderate only |
| Create/update products/categories | Deny | Deny | Allow |
| Manage coupons/banners | Deny | Deny | Allow |
| Change stock/order status | Deny | Deny | Allow + audit |
| View admin reports | Deny | Deny | Allow |

- MUST apply `[Authorize]`/policies on every protected controller/action. Use `[AllowAnonymous]` intentionally, not by default for Admin.
- MUST check **resource ownership at query/service layer**: `Order.UserId == authenticatedUserId`; a guessed order ID must never reveal data.
- MUST use an unguessable, limited-purpose tracking token or OTP verification for guest order status; order number alone is insufficient.
- MUST protect against IDOR (insecure direct object references) for order, address, invoice, review, uploaded image and custom request IDs.
- MUST restrict Admin configuration actions separately when later introducing staff roles.

## Inputs, output and files

- MUST validate all incoming DTO/ViewModels on server: length, numeric range, enum values, allowed transitions, file limits, address/phone sanity. Client validation is convenience, not security.
- MUST use Razor HTML encoding by default; prohibit untrusted `Html.Raw` and unsafe HTML storage/rendering. Sanitize rich text if ever added.
- MUST use EF Core parameterized query APIs; no string-concatenated SQL containing user input.
- MUST enforce upload allowlist (type/extension), verify actual file content/signature, size/dimension limits, random storage filenames, safe storage path, malware scanning where possible; do not trust original filename or MIME alone. Restrict who can fetch customer-uploaded reference images.
- MUST use anti-CSRF protection for cookie-authenticated mutations and appropriate CORS policy if external clients are added.
- MUST avoid reflecting exception stack traces, SQL or secret configuration in user errors.

## Payments, pricing and fraud controls

- MUST use a vetted gateway's hosted checkout/tokenization, HTTPS and official verification protocol. **Do not collect/store card PAN, CVV or PIN.**
- MUST verify gateway callback signatures/secrets as applicable **and** confirm server-to-server gateway status, amount, currency, order reference, merchant account and transaction ID.
- MUST make payment callbacks idempotent using unique constraints; replays cannot mark different orders paid.
- MUST calculate product price, discount, shipping, taxes/fees (if applicable) and final amount exclusively server-side, then save an immutable order total snapshot.
- MUST validate coupon eligibility, validity, per-user/global usage and stacking rules within a concurrency-safe transaction.
- MUST not accept negative total, expired offer, duplicate payment, negative inventory or invalid order-state transitions.
- MUST log critical status transitions without storing gateway secrets or personal financial credentials.

## Customer data & secrets

- MUST collect only checkout/account information genuinely needed for delivery and support, restrict access and define retention/deletion policy appropriate to applicable laws/agreements.
- MUST never expose server credentials, gateway secrets, API keys or connection strings in frontend HTML, JavaScript, repository or logs.
- MUST keep dev/test credentials distinct from production; rotate compromised secrets; never commit `.env`, `secrets.json`, or production connection details.
- MUST secure and regularly test DB backups and restore; redact phone/address/email from public logging.
- SHOULD maintain dependency vulnerability scanning and security update process.

## Minimum security verification

- Anonymous Customer/Admin endpoints blocked correctly; Customer A cannot view/update Customer B orders.
- CSRF attempt fails, XSS test is safely encoded, uploaded executable/non-image content rejected.
- Brute-force login requests throttled; password reset token expires/cannot be reused.
- Gateway fake-success, amount mismatch and replay callbacks do not mark orders Paid.
- Duplicate coupons and checkout submissions do not over-discount, oversell or double-charge.
