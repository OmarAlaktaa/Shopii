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

## Configuration & Secrets

Committed `appsettings.json` files contain safe, portable defaults. Values are resolved
from `appsettings.json`, `appsettings.{Environment}.json`,
[.NET User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets)
(Development only), environment variables, and command-line arguments — later sources win.

- `ConnectionStrings:DefaultConnection` is `Data Source=ShopiiDb.db` (relative path).
- `DevAuth` in `appsettings.Development.json` uses a placeholder identity
  (`dev@shopii.local` / `00000000-0000-0000-0000-000000000001`).
- `Jwt:Issuer`, `Jwt:Audience`, and `Jwt:AccessTokenMinutes` have safe defaults.
- `Jwt:Key` has **no committed value**. It must be supplied locally and in production.

Relative SQLite paths resolve against the **process working directory**: the repository
root when you run `dotnet run --project Shopii.API` from the root, or `Shopii.API/` when
running from inside that folder / via Visual Studio. Use an absolute path if you want a
single fixed database file.

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
