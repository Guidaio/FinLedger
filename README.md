# FinLedger

Double-entry ledger service for financial transactions. Portfolio project aligned with Senior .NET Backend (fintech/POS) roles.

## Context

Microservice that maintains a **double-entry ledger** for financial transactions: credit/debit per account, consistent balance, immutable history. Use case: internal ledger for a payment system or POS.

## Architecture

| Project | Description |
|---------|-------------|
| **FinLedger.Api** | Web API (.NET 8, minimal APIs) |
| **FinLedger.Domain** | Domain entities (Account, Transaction, Entry) |
| **FinLedger.Infrastructure** | EF Core, DbContext, persistence (SQLite) |

## Prerequisites

- .NET 8 SDK

## How to run

```bash
dotnet run --project src/FinLedger.Api
```

Swagger UI: http://localhost:5283/swagger (when running in Development).

## API (v1)

Base path: `/api/v1`

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | /accounts | Create account |
| GET | /accounts/{id}/balance | Get account balance |
| POST | /transactions | Create transaction (idempotent, use `Idempotency-Key` header) |

## Status

In development. See `portfolio-notes.md` for the roadmap and execution history.
