# Dependencies, Environments and Tooling

## Approved stack

- C#, ASP.NET Core MVC + Razor Views, ASP.NET Core Identity, EF Core, Bootstrap 5, HTML5/CSS3/JavaScript.
- Database: deliberately select MySQL 8.x or MariaDB (e.g., XAMPP installation). XAMPP is **optional** local DB convenience; it does not run the ASP.NET Core app. Kestrel/Visual Studio runs .NET application.
- A supported IDE and matching .NET SDK; Visual Studio, MySQL Workbench (MySQL), Git/GitHub, Chrome/Edge devtools; optional Postman and diagrams.net.

## NuGet and version requirements

- Required or common: `Microsoft.AspNetCore.Identity.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore.Design`, a **compatible** relational provider (`Pomelo.EntityFrameworkCore.MySql` for supported EF Core/MySQL/MariaDB combinations), test packages (xUnit + Microsoft.AspNetCore.Mvc.Testing).
- Optional: `Serilog.AspNetCore` (structured logs), `QuestPDF` (invoices; check license), gateway official SDK/HTTP integration, chart library and Bootstrap Icons/Swiper/SweetAlert2 where genuinely needed.
- **MUST verify a single supported compatibility matrix** (target .NET version, EF Core major version, Pomelo/provider release, MySQL/MariaDB server version) before package installation. Do not mix EF Core major versions or assume a provider supports an unreleased/newer major version. Lock versions in repo and record them in an ADR.
- Prefer minimal dependencies, reputable sources and licenses. Avoid redundant packages when built-in framework already provides feature.

## Environments

| Setting | Local Development | Staging | Production |
|---|---|---|---|
| DB | Local non-sensitive data | Test/sanitized DB | Restricted production DB |
| Gateway | Sandbox only | Sandbox/test verification | Approved live credentials |
| HTTPS | Dev certificate | Always | Always |
| Secrets | dotnet user-secrets/env vars | Secret store/env vars | Secret store/env vars |
| Errors | Debug details for developers | Private logs | Safe public error page |
| Background jobs | Test mode | Test mode | Monitored and durable |
| Backups | Developer snapshots | Restore rehearsal | Encrypted off-host backups |

Use `appsettings.json` for **non-secret defaults only**. Never commit actual gateway secrets, private credentials or live connection strings. Example keys: `ConnectionStrings:DefaultConnection`, `Payment:Mode`, `Store:Currency=BDT`, `Delivery:Zones`, `Email:SenderName`. Prefer strongly typed options with validation.

## Deployment

- Host ASP.NET Core via managed Windows/IIS hosting or Linux Kestrel + reverse proxy, with proper proxy header configuration, HTTPS and secure secrets.
- Choose persistently backed file/object storage; container filesystems can be ephemeral.
- Configure domain, HTTPS, DNS, health checks, restart/rollback, log retention, database backup, availability monitoring.
- Choose an approved Bangladesh-supported hosted gateway and satisfy merchant onboarding requirements before enabling live transactions.
