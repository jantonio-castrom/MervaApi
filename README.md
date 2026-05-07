# Merva Backend

ASP.NET Core 9.0 REST API for the Merva personal finance tracking application. Handles anonymous token-based authentication, encrypted expense/income storage, and device fingerprinting.

## Tech Stack

- **Framework**: ASP.NET Core 9.0 (C#)
- **Database**: SQL Server via EF Core Code-First
- **Auth**: Custom anonymous Bearer token scheme
- **Encryption**: AES-256-CBC per-field encryption
- **Deployment**: Docker, Azure App Service, Azure Pipelines

## Prerequisites

- .NET 9 SDK
- SQL Server (local or Azure SQL)
- Base64-encoded 32-byte encryption key for `USERTOKEN_KEY`

## Getting Started

1. Set connection string and encryption key in `MervaApi/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "MervaDb": "Server=localhost;Database=MervaDb;Trusted_Connection=True;"
  },
  "USERTOKEN_KEY": "<base64-encoded 32-byte key>"
}
```

2. Run the API:

```bash
dotnet run --project MervaApi/MervaApi.csproj
# HTTP:  http://localhost:5157
# HTTPS: https://localhost:7236
# Swagger UI: https://localhost:7236/swagger  (Development only)
```

## Docker

```bash
docker build -t merva-api .
docker run -p 8080:8080 \
  -e ConnectionStrings__MervaDb="<connection-string>" \
  -e USERTOKEN_KEY="<base64-key>" \
  merva-api
```

## API Endpoints

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| `GET` | `/home` | Required | Health check |
| `POST` | `/tokens/register` | None | Register or re-register a token |
| `POST` | `/tokens/validate` | None | Validate an existing token |
| `GET` | `/tokens/{token}` | None | Get token details |
| `GET` | `/expenses` | Required | List expenses for the current token |
| `POST` | `/expenses` | Required | Add a new expense |
| `GET` | `/incomes` | Required | List incomes for the current token |

Authentication: `Authorization: Bearer <token>` on all protected endpoints.

## Project Structure

```
MervaBackend/
├── MervaApi/              # ASP.NET Core Web API
│   ├── Data/              # EF Core DbContext (MervaDbContext)
│   ├── Encryption/        # AES-256-CBC encryption service
│   ├── Security/          # AnonymousToken auth handler
│   ├── UserTokens/        # Token + device management
│   ├── UserExpenses/      # Expense CRUD
│   ├── UserIncomes/       # Income CRUD
│   └── UserPreferences/   # User settings (theme, currency)
└── MervaDB/               # SQL Server schema project (DACPAC)
```

## CI/CD

Azure Pipelines (`azure-pipelines.yml`) triggers on `master`:

1. Restore → Build (Release) → Test → Publish
2. Artifacts: `dotnet-api` (binaries) + `merva-dacpac` (DB schema)

Production API: `https://mervaapi.azurewebsites.net`
