# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Merva is a full-stack application:
- **API**: ASP.NET Core 9.0 Web API in `MervaApi/`
- **DB**: SQL Server project in `MervaDB/` (schema-only, used with EF Core Code-First)
- **Client**: Angular 20 SPA at `../MervaWeb/` (sibling directory)

Hosted on Azure DevOps at `dev.azure.com/MenrvaServices/Merva`. Production API: `https://mervaapi.azurewebsites.net`.

## Commands

### API (`MervaApi/`)
```bash
dotnet build
dotnet run --project MervaApi/MervaApi.csproj          # HTTP :5157, HTTPS :7236
dotnet run --project MervaApi/MervaApi.csproj --launch-profile https
dotnet test
```

Use `MervaApi/MervaApi.http` to test endpoints with the REST client (file may contain stale routes — verify against controllers).

### Docker (`MervaBackend/`)
```bash
docker build -t merva-api .
docker run -p 8080:8080 -e ConnectionStrings__MervaDb="<connection-string>" merva-api
```
The image uses `mcr.microsoft.com/dotnet/aspnet:9.0` and listens on port 8080.

### Client (`../MervaWeb`)
```bash
npm start            # dev server at http://localhost:4200 (uses local config + proxy to :5157)
npm test             # Karma + Jasmine unit tests
npm run build        # production build to dist/merva-client/
```

## Architecture

### API (`MervaApi/`)

Code is organized by domain feature folder rather than by layer:

```
MervaApi/
├── Home/
│   ├── Controllers/      # HomeController
│   └── Models/           # HomeResponse
├── Data/                 # MervaDbContext (EF Core)
├── Encryption/Services/  # IEncryptionService, EncryptionService
├── Security/             # AnonymousTokenAuthHandler
├── Auth/                 # (reserved, currently empty)
├── UserTokens/
│   ├── Controllers/      # TokensController
│   ├── Models/           # UserToken, UserDevice entities + request records
│   └── Services/         # IUserTokenService, UserTokenService
├── UserExpenses/
│   ├── Controllers/      # ExpensesController
│   ├── Models/           # Expense entity, AddExpenseRequest, ExpenseResponse
│   └── Services/         # IUserExpenseService, UserExpenseService
├── UserIncomes/
│   ├── Controllers/      # IncomesController
│   ├── Models/           # UserIncome entity, IncomeResponse
│   └── Services/         # IUserIncomeService, UserIncomeService
└── UserPreferences/
    └── Models/           # UserPreference entity (no controller yet)
```

**Middleware / Services registered in `Program.cs`:**
- CORS — allows `http://localhost:4200`
- `MervaDbContext` — EF Core with SQL Server (connection string `MervaDb` from config)
- `EncryptionService` — singleton; requires `USERTOKEN_KEY` in config (Base64-encoded 32-byte value)
- `UserTokenService` — scoped
- `UserExpenseService` — scoped
- `UserIncomeService` — scoped
- Authentication scheme `"AnonymousToken"` — handled by `AnonymousTokenAuthHandler`
- Swagger/SwaggerUI — Development only

**Controllers:**
- `HomeController` — `GET /home` → returns `HomeResponse` (message + timestamp)
- `TokensController` — Token + device management:
  - `POST /tokens/register` — Upsert token: creates a new `UserToken` if the token is unknown, then always records a new `UserDevice` row (skipped if all tracked device fields match the most recent record). Returns `201 Created` for new tokens, `200 OK` for returning ones.
  - `POST /tokens/validate` — Check token exists
  - `GET /tokens/{token}` — Retrieve token details
- `ExpensesController`:
  - `GET /expenses?months={n}` — Returns decrypted, non-deleted expenses for the authenticated token within the last `n` months (default `3`), ordered by date desc
  - `POST /expenses` — Add expense (token resolved from `Authorization` header via auth handler)
  - `DELETE /expenses/{id}` — Soft-deletes the expense (sets `IsDeleted = true`, `DeletedAt = UtcNow`); returns `204 No Content` or `404` if not found / belongs to a different token
- `IncomesController`:
  - `POST /incomes` — Add income (token resolved from request body; returns `201 Created` with income ID, or `401` if token unknown)
  - `GET /incomes` — Returns all decrypted, non-deleted incomes for the authenticated token, ordered by date desc
  - `DELETE /incomes/{id}` — Soft-deletes the income (sets `IsDeleted = true`, `DeletedAt = UtcNow`); returns `204 No Content` or `404` if not found / belongs to a different token

**Authentication (`Security/AnonymousTokenAuthHandler`):**

Custom `AuthenticationHandler` for the `"AnonymousToken"` scheme. On every request:
1. Reads the `Authorization: Bearer <token>` header
2. Computes SHA-256 hash of the raw token
3. Looks up the hash in `UserTokens.EncryptedValueHash` (no decryption needed)
4. On success, sets an `AnonymousTokenId` claim on the principal (value = `TokenId`)
5. Returns 401 on missing header, bad format, or unknown token

All controllers are protected with `[Authorize]`. The `AnonymousTokenId` claim is used downstream by services to scope data to the correct token row.

**Models:**
- `UserToken` — Entity; stores encrypted token (`Token` NVARCHAR(64)), `EncryptedValueHash` (VARBINARY(32), unique), and `CreatedAt`. Device fingerprint data lives in `UserDevice` (separate table).
- `UserDevice` — Entity; 1:N with `UserToken`. Records each distinct device session: `UserAgent`, `Browser`, `BrowserVersion`, `OperatingSystem`, `Language`, `Timezone`, `IpAddress`, `Country`, `Region`, `City`, `Isp`, `ConnectionType`, `RecordedAt`. A new row is only inserted when at least one of the tracked fields differs from the most recent existing record for that token.
- `UserPreference` — Entity; 1:1 with `UserToken`; fields: `DefaultCurrency` (default `"USD"`), `Theme` (default `"light"`), `UpdatedAt`
- `Expense` — Entity; 1:N with `UserToken`; `Name`, `Amount`, `Currency`, `Category` stored as encrypted `string` (NVARCHAR(MAX)); `ExpenseDate` (DateOnly), `CreatedAt`; soft-delete fields `IsDeleted` (bool, default `false`) and `DeletedAt` (DateTime?). A global EF Core query filter in `MervaDbContext` (`HasQueryFilter(e => !e.IsDeleted)`) automatically excludes soft-deleted rows from all queries.
- `UserIncome` — Entity; 1:N with `UserToken`; same encrypted field pattern as `Expense`; `IncomeDate` (DateOnly), `CreatedAt`; soft-delete fields `IsDeleted` (bool, default `false`) and `DeletedAt` (DateTime?). A global EF Core query filter automatically excludes soft-deleted rows from all queries.
- `ExpenseResponse` / `IncomeResponse` — Response records returned by GET endpoints with all fields decrypted; `Amount` parsed back to `decimal`
- Request records (`RegisterTokenRequest`, `ValidateTokenRequest`, `AddExpenseRequest`, `AddIncomeRequest`) live in their feature's `Models/` folder

**Services:**
- `EncryptionService` — AES-256-CBC, random IV per call (prepended to ciphertext). Key sourced from `USERTOKEN_KEY` config (Base64-encoded 32-byte value). Also exposes `ComputeSha256` used for token lookups.
- `UserTokenService` — All token DB logic:
  - `TokenExistsAsync` — checks by encrypted `Token` column
  - `RegisterAsync(request, ipAddress)` — upserts the `UserToken` then conditionally inserts a `UserDevice` row; returns `(UserToken Token, bool IsNew)`. Device insert is skipped when all 11 tracked fields match the most recent device row.
  - `GetByTokenAsync` — looks up by `EncryptedValueHash`, decrypts and returns token value
- `UserExpenseService` — `AddExpenseAsync` encrypts all fields and inserts; `GetExpensesAsync(tokenId, fromDate?)` fetches non-deleted rows on or after `fromDate` (when provided) ordered by date desc, decrypts each field, returns `IReadOnlyList<ExpenseResponse>` (soft-deleted rows excluded automatically via global query filter); `SoftDeleteExpenseAsync(expenseId, tokenId)` marks the row as deleted and returns `false` if not found or owned by a different token
- `UserIncomeService` — `AddIncomeAsync` encrypts all fields and inserts; `GetIncomesAsync(tokenId)` fetches all non-deleted rows ordered by date, decrypts each field, returns `IReadOnlyList<IncomeResponse>` (soft-deleted rows excluded automatically via global query filter); `SoftDeleteIncomeAsync(incomeId, tokenId)` marks the row as deleted and returns `false` if not found or owned by a different token

### Database (`MervaDB/`)

EF Core Code-First with Fluent API configuration in `MervaDbContext`. SQL Server project in `MervaDB/Tables/` mirrors the schema for tracking/diffing.

| Table | Key points |
|---|---|
| `UserTokens` | PK `TokenId`; unique index on `Token`; unique constraint on `EncryptedValueHash` |
| `UserDevices` | PK `DeviceId`; FK `TokenId → UserTokens`; all device fields nullable; `RecordedAt` defaults to `GETUTCDATE()` |
| `Expenses` | PK `ExpenseId`; FK `TokenId → UserTokens`; all value columns `NVARCHAR(MAX)` (encrypted); `IsDeleted BIT NOT NULL DEFAULT 0`; `DeletedAt DATETIME2 NULL` |
| `UserIncomes` | PK `IncomeId`; FK `TokenId → UserTokens`; same column pattern as `Expenses` |
| `UserPreferences` | PK `PreferenceId`; FK `TokenId → UserTokens` (1:1 unique); `DefaultCurrency` NCHAR(3), `Theme` NVARCHAR(20) |

### CI/CD (`azure-pipelines.yml`)

Triggers on `master`. Windows agent, Release config:
1. Install .NET 9 SDK
2. `dotnet restore`
3. `dotnet build -c Release`
4. `dotnet test` (matches `**/*Tests/*.csproj`)
5. `dotnet publish` → staging dir → artifact `dotnet-api`
6. Publishes DB schema artifact `merva-dacpac` from `MervaDB/`

## Azure Guidelines

Follow Azure best practices for any new integrations (storage, service bus, identity). Prefer Azure SDK packages and managed identity over connection strings where possible.
