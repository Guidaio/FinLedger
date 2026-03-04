# FinLedger

Double-entry ledger service for financial transactions. Portfolio project aligned with Senior .NET Backend (fintech/POS) roles.

## Context

FinLedger is a microservice that maintains a **double-entry ledger** for financial transactions. It ensures credit/debit consistency per account, immutable transaction history, and accurate balance calculation.

**Use case:** Internal ledger for a payment system, POS (Point of Sale), or fintech platform. Each transaction (e.g. sale, refund, transfer) is recorded as balanced entries across accounts (merchant, cash, revenue, etc.).

**Double-entry rule:** Every transaction has at least two entries; the sum of debits equals the sum of credits.

## Architecture

```
┌─────────────────┐     ┌──────────────────────┐     ┌─────────────────────┐
│   FinLedger.Api │────▶│ FinLedger.Infrastructure│────▶│  FinLedger.Domain   │
│  (minimal APIs) │     │  (EF Core, SQLite)     │     │ (Account, Transaction│
│  /api/v1/*      │     │  LedgerDbContext       │     │  Entry, BalanceCalc)│
└─────────────────┘     └──────────────────────┘     └─────────────────────┘
```

| Project | Description |
|---------|-------------|
| **FinLedger.Api** | Web API (.NET 8, minimal APIs). Endpoints, ProblemDetails, Swagger, versioning. |
| **FinLedger.Domain** | Domain entities (Account, Transaction, Entry), enums (AccountType, EntrySide), BalanceCalculator. |
| **FinLedger.Infrastructure** | EF Core, LedgerDbContext, SQLite persistence, Fluent API configurations. |

### Domain model

| Entity | Description |
|--------|-------------|
| **Account** | Ledger account (merchant, cash, revenue, etc.). Has Code (unique), Type (Asset, Liability, Equity, Revenue, Expense). |
| **Transaction** | Financial transaction. IdempotencyKey for safe retries. Must have balanced entries. |
| **Entry** | Single debit or credit linked to a Transaction and an Account. |

**Balance calculation:** Asset/Expense = debits − credits. Liability/Equity/Revenue = credits − debits.

## Prerequisites

- .NET 8 SDK

## Configuration

Connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=finledger.db"
  }
}
```

## How to run

```bash
dotnet run --project src/FinLedger.Api
```

Swagger UI: http://localhost:5283/swagger (Development).

## API (v1)

Base path: `/api/v1`

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | /accounts | Create account (Name, Code, Type) |
| GET | /accounts/{id}/balance | Get account balance |
| POST | /transactions | Create transaction (idempotent; use `Idempotency-Key` header) |

### Example: create account

```bash
curl -X POST http://localhost:5283/api/v1/accounts \
  -H "Content-Type: application/json" \
  -d '{"name":"Cash","code":"CASH","type":0}'
```

### Example: create transaction

```bash
curl -X POST http://localhost:5283/api/v1/transactions \
  -H "Content-Type: application/json" \
  -H "Idempotency-Key: tx-001" \
  -d '{
    "description":"Sale",
    "entries":[
      {"accountId":"<cash-id>","amount":100,"side":0},
      {"accountId":"<revenue-id>","amount":100,"side":1}
    ]
  }'
```

Side: 0 = Debit, 1 = Credit.

## Running tests

```bash
dotnet test
```

Unit tests: BalanceCalculator. Integration tests: API endpoints with WebApplicationFactory and InMemory database.

## Technologies

- .NET 8
- ASP.NET Core minimal APIs
- Entity Framework Core 8 (SQLite)
- Swagger / OpenAPI
- xUnit, Microsoft.AspNetCore.Mvc.Testing

## Status

In development. See `portfolio-notes.md` for the roadmap and execution history.
