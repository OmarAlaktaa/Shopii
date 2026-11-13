# Shopii

A .NET 8 e-commerce API built with Clean Architecture (API / Application / Domain / Infrastructure),
EF Core with SQLite, JWT authentication, and MediatR.

## Projects

| Project | Responsibility |
| --- | --- |
| `Shopii.API` | ASP.NET Core Web API, controllers, auth pipeline |
| `Shopii.Application` | Use cases, interfaces, DTOs |
| `Shopii.Domain` | Entities and domain rules |
| `Shopii.Infrastructure` | EF Core persistence, repositories, JWT token service |
| `Shopii.Tests` | Automated tests |

## Running locally

```powershell
dotnet run --project Shopii.API
```

Swagger is available at `/swagger` in the Development environment.

## Logging & audit trail

The API uses Serilog for diagnostics and a separate SQLite audit table for
security- and business-sensitive actions.

### Diagnostic logs

- **Console**: compact bracketed format with correlation id, trace id, and user id.
- **Rolling file**: `logs/shopii-YYYYMMDD.log` relative to the process working
  directory (usually `Shopii.API/logs` when running via `dotnet run --project`).
  Files roll daily, keep 14 files, and are capped at 50 MB each.
- **Seq**: `http://localhost:5341` when Seq is running. General application logs
  belong in Seq, not in `ShopiiDb.db`.

Every request gets an `X-Correlation-Id` (or accepts an incoming
`X-Correlation-Id` / `X-Request-Id`). The value is echoed in the response and
added to every log line for that request, so use it to join events in Seq.

Request summaries log the method and path only; query strings, headers, request
bodies, passwords, tokens, email addresses, and card data are never logged.
Successful health probes are logged at `Verbose`, 4xx responses at `Warning`,
and 5xx responses at `Error` (including an unhealthy `/health` probe). If the
exception middleware already wrote the detailed error, the request summary is
downgraded to `Information` to avoid duplicate error entries.

`SensitiveDataDestructuringPolicy` is a defense-in-depth redaction layer for
objects accidentally logged with `{@Object}`.

Start Seq locally with Docker:

```powershell
docker run --name shopii-seq -d --restart unless-stopped `
  -e ACCEPT_EULA=Y -p 5341:80 datalust/seq:latest
```

Open http://localhost:5341. Set the retention policy to 30 days in Seq
(Settings → Data → Retention). Seq is intentionally not wrapped in
`Serilog.Sinks.Async`; it already batches asynchronously.

### Audit trail

Security- and business-sensitive actions are written to the `AuditLogs` table in
the SQLite database. Audit writes are best-effort: if the insert fails, the
failure is logged as `AuditWriteFailed` and the original request still succeeds.
The writer uses a dedicated DbContext scope and a short timeout so a failed audit
insert cannot corrupt the request's unit of work or hang the request.

Recorded actions include:

- `LoginSucceeded`, `LoginFailed`, `LoginRejected`, `UserRegistered`
- `UsersListViewed`, `UserProfileViewed`, `UserUpdated`, `OwnProfileUpdated`,
  `UserDeleted`
- `OrdersListViewed`, `OrderViewed`, `OrderCreated`, `OrderUpdated`,
  `OrderStatusChanged`, `OrderArchived`
- `ProductCreated`, `ProductUpdated`, `ProductDeleted`

Each row contains the timestamp, actor user id (nullable for anonymous actions),
action, entity type/id, correlation id, and a small JSON details payload. Callers
build details through `AuditDetails`, which only accepts scalar values, caps the
number and length of fields, and redacts keys such as `Email`, `Password`,
`Token`, and `CardNumber`. Details never include passwords, tokens, email
addresses, request bodies, query strings, or headers.

Audit rows are retained for 365 days by default:

```json
"Audit": {
  "RetentionDays": 365
}
```

Set `Audit:RetentionDays` to `0` to disable automatic cleanup. The cleanup
service runs at startup and every 24 hours.

There is deliberately no admin HTTP endpoint for audit data; query the table
directly with a SQLite client:

```sql
SELECT Timestamp, ActorUserId, Action, EntityType, EntityId, CorrelationId, Details
FROM AuditLogs
ORDER BY Timestamp DESC
LIMIT 100;
```

## Configuration & Secrets

Committed `appsettings.json` files contain safe, portable defaults. Values are resolved
from `appsettings.json`, `appsettings.{Environment}.json`,
[.NET User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets)
(Development only), environment variables, and command-line arguments — later sources win.

- `ConnectionStrings:DefaultConnection` is `Data Source=../ShopiiDb.db` (relative path).
- `DevAuth` in `appsettings.Development.json` uses a placeholder identity
  (`dev@shopii.local` / `00000000-0000-0000-0000-000000000001`).
- `Jwt:Issuer`, `Jwt:Audience`, and `Jwt:AccessTokenMinutes` have safe defaults.
- `Jwt:Key` has **no committed value**. It must be supplied locally and in production.

Relative SQLite paths resolve against the **application content root**, which is the
`Shopii.API/` folder for `dotnet run --project Shopii.API` and Visual Studio. The default
`../ShopiiDb.db` therefore points at the repository root. Use an absolute path if you want
a single fixed database file regardless of how the app is started.

Do **not** commit keys, personal connection strings, emails, or user ids. Override them
locally instead, either with .NET User Secrets (Development only, stored outside the repo)
or with environment variables.

### JWT signing key (required)

`Jwt:Key` is a Base64-encoded key used to sign and validate JWTs. It must decode to at
least **32 bytes (256 bits)** because tokens are signed with HMAC-SHA256. The API refuses
to start when the key is missing, empty, not valid Base64, or too short, and the error
message explains how to fix it.

`appsettings.Development.example.json` documents the complete expected `Jwt` section, but
it is a reference only — ASP.NET Core does not load `*.example.json`. Never put a real key
in it.

Generate a key (PowerShell):

```powershell
$bytes = New-Object byte[] 32
[System.Security.Cryptography.RandomNumberGenerator]::Fill($bytes)
[Convert]::ToBase64String($bytes)
```

Store it with User Secrets. `Shopii.API.csproj` already declares a `UserSecretsId`, so no
`init` step is needed:

```powershell
dotnet user-secrets set "Jwt:Key" "<your-key>" --project Shopii.API
```

User Secrets are loaded automatically only in the Development environment.

### Other User Secrets settings

```powershell
# Point at your own database file (absolute path recommended)
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Data Source=C:\path\to\ShopiiDb.db" --project Shopii.API

# Optional: use your own development identity
dotnet user-secrets set "DevAuth:UserId" "<your-guid>" --project Shopii.API
dotnet user-secrets set "DevAuth:Email" "you@example.com" --project Shopii.API
dotnet user-secrets set "DevAuth:Role" "Admin" --project Shopii.API
```

Useful commands:

```powershell
dotnet user-secrets list --project Shopii.API
dotnet user-secrets remove "DevAuth:Email" --project Shopii.API
dotnet user-secrets clear --project Shopii.API
```

### Other environment variables

Use a double underscore (`__`) to separate configuration sections. These work in every
environment.

```powershell
$env:ConnectionStrings__DefaultConnection = "Data Source=C:\path\to\ShopiiDb.db"
$env:DevAuth__UserId = "<your-guid>"
$env:DevAuth__Email = "you@example.com"
$env:DevAuth__Role = "Admin"
```

PowerShell sets these for the current session only. To persist them:

```powershell
[Environment]::SetEnvironmentVariable("ConnectionStrings__DefaultConnection", "Data Source=C:\path\to\ShopiiDb.db", "User")
```

### Production, Docker, and cloud deployment

Production has no User Secrets. Provide the key as an environment variable using the
double-underscore form `Jwt__Key`, which maps to the `Jwt:Key` configuration key. The value
must be the same Base64 string on a single line, without surrounding quotes.

Bash / Linux:

```bash
export Jwt__Key="<your-key>"
dotnet Shopii.API.dll
```

PowerShell:

```powershell
$env:Jwt__Key = "<your-key>"
dotnet .\Shopii.API.dll
```

Docker:

```bash
docker run -e Jwt__Key="<your-key>" -p 8080:8080 shopii-api
```

Docker Compose:

```yaml
services:
  shopii-api:
    image: shopii-api
    environment:
      Jwt__Key: ${JWT__KEY}
```

Keep the actual value in a local `.env` file (ignored by Git) or your CI/CD secret store,
and inject it at deploy time. Never bake it into an image layer or commit it.

**Azure App Service**: open Configuration → Application settings and add a setting named
`Jwt__Key` with the Base64 value (use `Jwt__Key` rather than `Jwt:Key` so it behaves the
same on Linux and Windows plans). Alternatively, reference a Key Vault secret:

```text
Jwt__Key = @Microsoft.KeyVault(SecretUri=https://<vault>.vault.azure.net/secrets/Jwt-Key/)
```

The App Service needs a system-assigned or user-assigned managed identity with the
`Key Vault Secrets User` role on the vault. No application-code change or extra NuGet
package is needed: App Service resolves the reference and exposes it as the `Jwt__Key`
environment variable.

**Other clouds / orchestrators**: inject the secret into `Jwt__Key` via Azure Container Apps
secrets, a Kubernetes Secret exposed as an environment variable, GitHub Actions or Azure
DevOps secret variables, AWS Secrets Manager, and so on.

Rotating the key invalidates all existing access tokens, so issue new ones after deploying
a new key.

### Dev authentication

In Development the API injects a fake authenticated user for requests without an
`Authorization` header. The identity comes from `DevAuth` and can be overridden per
request with the `X-Dev-User-Id`, `X-Dev-Role`, and `X-Dev-Email` headers. This bypass is
never registered outside Development.

## SQLite files

Local database files (`*.db`, `*.db-shm`, `*.db-wal`) are ignored by `.gitignore` and
should never be committed.
