# MervaDB

SQL Server Data Tools (SSDT) project for the Merva database schema. Compiles to a DACPAC used for versioned deployments via Azure Pipelines. The authoritative schema is EF Core Code-First in `../MervaApi/Data/MervaDbContext.cs` — these SQL files mirror it for diffing and CI/CD artifacts.

## Tech Stack

- **Project type**: SSDT (.sqlproj), targeting Azure SQL (SqlAzureV12)
- **Output**: `bin/Debug/MervaDB.dacpac`
- **Deployment**: `LocalPublish.publish.xml` for local, Azure Pipelines for production

## Local Deployment

Open in Visual Studio and publish via `LocalPublish.publish.xml`, or use SqlPackage:

```bash
sqlpackage /Action:Publish \
  /SourceFile:bin/Debug/MervaDB.dacpac \
  /TargetServerName:localhost \
  /TargetDatabaseName:MervaDB-Local \
  /TargetTrustServerCertificate:true
```

## Schema

All tables are in the `dbo` schema. Financial fields (`Name`, `Amount`, `Currency`, `Category`) are stored as `NVARCHAR(MAX)` encrypted at rest — decryption is handled by the API layer.

### Relationships

```
UserTokens (PK: TokenId)
├── 1 → * UserDevices
├── 1 → * Expenses
├── 1 → * UserIncomes
└── 1 → 1 UserPreferences
```

### UserTokens

| Column | Type | Notes |
|--------|------|-------|
| `TokenId` | INT IDENTITY | PK |
| `Token` | NVARCHAR(MAX) | AES-256-CBC encrypted token value |
| `EncryptedValueHash` | VARBINARY(32) | SHA-256 hash; unique index; used for lookups |
| `CreatedAt` | DATETIME2 | Default `GETUTCDATE()` |

### UserDevices

| Column | Type | Notes |
|--------|------|-------|
| `DeviceId` | INT IDENTITY | PK |
| `TokenId` | INT | FK → `UserTokens` |
| `UserAgent` | NVARCHAR(500) | Nullable |
| `Browser` | NVARCHAR(100) | Nullable |
| `BrowserVersion` | NVARCHAR(50) | Nullable |
| `OperatingSystem` | NVARCHAR(100) | Nullable |
| `Language` | NVARCHAR(20) | Nullable |
| `Timezone` | NVARCHAR(100) | Nullable |
| `IpAddress` | NVARCHAR(45) | Nullable; IPv4 and IPv6 |
| `Country` | NVARCHAR(100) | Nullable |
| `Region` | NVARCHAR(100) | Nullable |
| `City` | NVARCHAR(100) | Nullable |
| `Isp` | NVARCHAR(200) | Nullable |
| `ConnectionType` | NVARCHAR(50) | Nullable |
| `RecordedAt` | DATETIME2 | Default `GETUTCDATE()` |

### Expenses

| Column | Type | Notes |
|--------|------|-------|
| `ExpenseId` | INT IDENTITY | PK |
| `TokenId` | INT | FK → `UserTokens` |
| `Name` | NVARCHAR(MAX) | Encrypted |
| `Amount` | NVARCHAR(MAX) | Encrypted; parsed to decimal by API |
| `Currency` | NVARCHAR(MAX) | Encrypted |
| `Category` | NVARCHAR(MAX) | Encrypted; nullable |
| `ExpenseDate` | DATE | |
| `CreatedAt` | DATETIME2 | Default `GETUTCDATE()` |

### UserIncomes

Identical structure to `Expenses` with `IncomeId` / `IncomeDate` column names.

### UserPreferences

| Column | Type | Notes |
|--------|------|-------|
| `PreferenceId` | INT IDENTITY | PK |
| `TokenId` | INT | FK → `UserTokens`; unique (1:1) |
| `DefaultCurrency` | NCHAR(3) | Default `'USD'` |
| `Theme` | NVARCHAR(20) | Default `'light'` |
| `UpdatedAt` | DATETIME2 | |

## CI/CD

The `azure-pipelines.yml` in `../` publishes this project's DACPAC as the `merva-dacpac` artifact on every `master` build.
