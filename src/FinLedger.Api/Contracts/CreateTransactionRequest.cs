using FinLedger.Domain.Entities;

namespace FinLedger.Api.Contracts;

public record CreateTransactionRequest(string? IdempotencyKey, string? Description, List<EntryRequest> Entries);

public record EntryRequest(Guid AccountId, decimal Amount, EntrySide Side);
