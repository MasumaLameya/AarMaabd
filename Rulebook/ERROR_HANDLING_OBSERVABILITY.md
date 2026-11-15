# Error Handling, Logging and Operational Reliability

## Error handling principles

1. Fail safely and explain clearly; never show stack traces, SQL, gateway responses with secrets, or internal file paths to customers.
2. Validate inputs early with `ModelState`; render field-level messages (and a summary where appropriate) while preserving non-sensitive form state.
3. Handle expected business failures (`OutOfStock`, `InvalidCoupon`, `UnavailableDeliverySlot`, `PaymentNotVerified`, `Unauthorized`) as typed service results or known exceptions.
4. Let unexpected errors go to centralized ASP.NET Core exception handler (`UseExceptionHandler`) and log with correlation ID.
5. Do not silently catch `Exception` and return success/default values.
6. Use consistent page statuses: 400 bad input, 401 unauthenticated, 403 unauthorized, 404 missing resource, 409 conflict, 429 throttled, 500 server error, 503 temporary dependency outage, where appropriate.
7. Redirect to an accessible friendly error page for HTML flows; JSON endpoints return consistent structured problem details (`ProblemDetails`), excluding secrets.

## Business outcome → response

| Situation | Customer-facing outcome | Server behavior |
|---|---|---|
| Invalid form | Explain relevant fields | No write; log only if useful |
| Item out of stock | Update cart / try later | Atomic stock operation fails cleanly; rollback |
| Coupon expired/ineligible | Coupon message; correct total | No discount used; do not redeem |
| Duplicate Place Order click | Return same order/reference where safe | Idempotent request lookup |
| Gateway unsuccessful / unknown | Pending/failed; do not claim Paid | Verify/refetch; retry/compensate safely |
| Admin action forbidden | Access denied | HTTP 403 + security log |
| Database unavailable | Temporary unavailable message | Alert/log, no partial success claim |
| Unexpected server error | 'Something went wrong' + reference ID | Structured error log + 500 |

## Logging and correlation

- Use `ILogger<T>`; optionally Serilog for structured events and sinks. Include timestamp (UTC), environment, request correlation ID, action, safe order ID and status.
- Never log full passwords, authentication cookies, reset tokens, card details, gateway secrets, full private addresses, or unredacted personal data.
- Maintain separate **AuditLog** for sensitive admin changes (actor ID, action, target, timestamp, safe before/after metadata), e.g. pricing, coupon validity, manual payment override, order cancellation.
- Set appropriate log levels: Information business milestones, Warning business/security anomalies, Error technical failures.
- Monitoring/alerts should cover exception spikes, 5xx rates, delayed payment verification, failed backups and database unavailability.
- Protect log access and rotate/retain per approved policy.

## Fault tolerance and recovery

- Put timeouts on external calls; use short bounded retries with exponential backoff **only for safe/idempotent operations**.
- Never blindly retry order placement or charge creation without idempotency keys.
- Gateway callback can arrive late/out-of-order; match order, amount, currency and transaction reference before state transition.
- Background work (`IHostedService`) must use explicit scoped services, cancellation tokens, safe retry and persistence of important jobs/events as needed; not fire-and-forget controller tasks.
- Backup database regularly, store copies outside primary host, encrypt them and periodically **test restore**.
- Have runbooks for rollback, payment reconciliation, refund dispute, data breach response and failed deployment.

## Testing

- Unit tests cover failures/validations. Integration tests cover DB rollback. Simulate gateway timeout, duplicate callback, image upload failure, MySQL disconnect, duplicate checkout and two buyers competing for one item.
- Developer must inspect logs and user feedback. An absence of exceptions in a short demo does not prove system reliability.
