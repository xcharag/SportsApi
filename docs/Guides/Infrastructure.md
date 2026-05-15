# Infrastructure Layer

The infrastructure project (`SportsApi.infrastructure`) provides all concrete implementations for the abstractions defined in the domain. It wires up the database, authentication, authorization, internal messaging, and exception handling.

---

## Service Registration

```csharp
// Program.cs / startup
builder.Services.AddInfrastructure(builder.Configuration);
```

`InfrastructureServiceExtensions.AddInfrastructure()` registers everything below in one call.

---

## Database — EF Core + PostgreSQL

### Provider & Package

```xml
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.0.4" />
```

### Connection String

Configured in `appsettings.json` under `ConnectionStrings`. The extension tries `CoreConnection` first and falls back to `LocalConnection`:

```json
"ConnectionStrings": {
  "CoreConnection": "Host=localhost;Port=5432;Database=SportsApiDb;Username=postgres;Password=your_password_here"
}
```

For local development, add `LocalConnection` in `appsettings.Development.json`:

```json
"ConnectionStrings": {
  "LocalConnection": "Host=localhost;Port=5432;Database=SportsApiDb;Username=postgres;Password=dev_password"
}
```

### `CoreDbContext`

Inherits from `DbContext`. Registers the following `DbSet<T>` properties:

| DbSet | Entity |
|---|---|
| `Tournaments` | `Tournament` |
| `TeamParticipations` | `TeamParticipation` |
| `RoundsClassified` | `RoundsClassified` |
| `Teams` | `Team` |
| `Players` | `Player` |
| `Rosters` | `Roster` |
| `Matches` | `Match` |
| `Events` | `Event` |

All entity configurations are applied from the assembly via `modelBuilder.ApplyConfigurationsFromAssembly(...)`.

### Retry Policy

The connection is configured with automatic retry on failure:
- Max retry count: **5**
- Max retry delay: **10 seconds**

### Migrations

```powershell
# Add a new migration
dotnet ef migrations add <MigrationName> `
  --project SportsApi.infrastructure `
  --startup-project SportsApi.api

# Apply to database
dotnet ef database update `
  --project SportsApi.infrastructure `
  --startup-project SportsApi.api
```

---

## JWT Authentication

Configured in `appsettings.json` under `JwtSettings`:

```json
"JwtSettings": {
  "SecretKey": "...",   ← must match the auth microservice secret
  "Issuer":    "SisApi",
  "Audience":  "SisApiClient"
}
```

Or, for JWKS-based validation (OpenID Connect / external provider):

```json
"JwtSettings": {
  "Authority": "https://your-auth-server.com",
  "Audience":  "SisApiClient"
}
```

**Behaviour:**
- If `Authority` is set → JWKS discovery is used (no local secret needed).
- If only `SecretKey` is set → symmetric HS256 validation.
- The token is extracted from the `Authorization: Bearer <token>` header **or** the `accessToken` cookie.
- Authentication errors are stored under `HttpContext.Items["JwtAuthError"]` and exposed via `ICurrentUser.AuthenticationError`.

---

## `ICurrentUser` — Current User Service

`CurrentUserService` reads standard and custom JWT claims from `IHttpContextAccessor`.

| Property | Claim | Description |
|---|---|---|
| `UserId` | `NameIdentifier` | User's unique ID |
| `Username` | `Name` | Display name |
| `Email` | `EmailAddress` | Email address |
| `TokenId` | `jti` | Token ID |
| `CompanyId` | `CompanyId` | Custom claim |
| `Roles` | `Role` (all) | List of role names |
| `ExpiresAt` | `exp` | Unix timestamp expiry |
| `Issuer` | `iss` | Token issuer |
| `Audience` | `aud` | Token audience |
| `IsAuthenticated` | — | Whether the user is authenticated |
| `AuthenticationError` | — | JWT validation error, if any |

If a required claim is missing and the user is **not** authenticated, `UserId` and `Username` throw `UnauthorizedAccessException` with a descriptive message pointing at the likely cause (wrong secret, expired token, no token sent).

---

## Dynamic Permission System

Every authenticated request is checked against the external **SISAPI** authorization microservice.

### Flow

```
[Request arrives]
      │
      ▼
DynamicPermissionGlobalFilter (IAsyncAuthorizationFilter)
  ├─ [AllowAnonymous]?         → skip
  ├─ [SkipDynamicPermission]?  → skip permission check (JWT still required)
  ├─ Not authenticated?         → 401
  ├─ Extract Bearer token
  ├─ Resolve controller name (SwaggerTag → route → URL path)
  ├─ Map HTTP method → action (Read/Write/Update/Delete)
  └─ AuthMicroserviceClient.VerifyPermissionAsync(token, module, controller, action)
        ├─ true  → continue
        └─ false → 403 Forbidden
```

### SISAPI Configuration

```json
"SisApi": {
  "BaseUrl": "https://sisapi.xchar.site"
},
"DynamicPermission": {
  "Module": "SportsApi"
}
```

### `AuthMicroserviceClient`

Calls `GET /api/Auth/verify-permission?module=...&controller=...&action=...&typePermission=0` with the user's Bearer token. Returns `true` only if the response is `200 OK` and `data == true`.

### Static Permission Check — `[RequirePermission]`

For endpoints where you need to declare the required permission explicitly:

```csharp
[RequirePermission("SportsApi", "Tournaments", "Write")]
```

This creates an ASP.NET Core authorization policy named `Permission_SportsApi-Tournaments:Write`. `PermissionPolicyProvider` dynamically builds the policy and `PermissionAuthorizationHandler` checks that the user has a `Permission` claim matching that value.

---

## Global Exception Middleware

`GlobalExceptionMiddleware` catches unhandled exceptions and returns a consistent JSON error envelope:

```json
{ "status": 400, "error": "Human-readable message" }
```

| Exception | HTTP Status | Message |
|---|---|---|
| `UnauthorizedAccessException` | 401 | Authentication message |
| `DbUpdateException` (unique constraint) | 409 | Duplicate record message |
| `DbUpdateException` (other) | 400 | Data save error message |
| `ArgumentException` | 400 | Invalid data message |
| `InvalidOperationException` | 400 | Operation failed message |
| Any other | 500 | Unexpected error message |

Register it in `Program.cs`:

```csharp
app.UseMiddleware<GlobalExceptionMiddleware>();
```

---

## JSON Serialization

Configured globally on the MVC options:

| Setting | Value |
|---|---|
| `ReferenceHandler` | `IgnoreCycles` (prevents circular reference errors) |
| `IncludeFields` | `true` |
| `PropertyNamingPolicy` | `camelCase` |
| `DefaultIgnoreCondition` | `Never` (all properties serialized, including nulls) |

