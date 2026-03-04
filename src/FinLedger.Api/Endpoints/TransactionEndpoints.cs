using FinLedger.Api.Contracts;
using FinLedger.Domain.Entities;
using FinLedger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinLedger.Api.Endpoints;

public static class TransactionEndpoints
{
    public static void MapTransactionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/transactions").WithTags("Transactions");

        group.MapPost("/", CreateTransaction)
            .WithName("CreateTransaction")
            .WithOpenApi()
            .Produces<TransactionResponse>(StatusCodes.Status201Created)
            .Produces<TransactionResponse>(StatusCodes.Status200OK) // idempotent replay
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status422UnprocessableEntity);
    }

    private static async Task<IResult> CreateTransaction(
        CreateTransactionRequest request,
        LedgerDbContext db,
        HttpContext httpContext,
        CancellationToken ct)
    {
        // Idempotency: header takes precedence over body
        var idempotencyKey = httpContext.Request.Headers["Idempotency-Key"].FirstOrDefault()
            ?? request.IdempotencyKey;

        if (string.IsNullOrWhiteSpace(idempotencyKey))
            return Results.BadRequest(new { error = "Idempotency-Key header or request.IdempotencyKey is required" });

        idempotencyKey = idempotencyKey.Trim();

        // Idempotent: return existing transaction if key already used
        var existing = await db.Transactions
            .Include(t => t.Entries)
            .FirstOrDefaultAsync(t => t.IdempotencyKey == idempotencyKey, ct);

        if (existing is not null)
            return Results.Ok(ToResponse(existing));

        // Validate entries
        if (request.Entries is null || request.Entries.Count < 2)
            return Results.BadRequest(new { error = "At least 2 entries are required" });

        var totalDebits = request.Entries.Where(e => e.Side == EntrySide.Debit).Sum(e => e.Amount);
        var totalCredits = request.Entries.Where(e => e.Side == EntrySide.Credit).Sum(e => e.Amount);

        if (totalDebits != totalCredits)
            return Results.UnprocessableEntity(new
            {
                error = "Entries must balance: sum of debits must equal sum of credits",
                totalDebits,
                totalCredits
            });

        if (request.Entries.Any(e => e.Amount <= 0))
            return Results.BadRequest(new { error = "Amount must be positive" });

        // Validate all accounts exist
        var accountIds = request.Entries.Select(e => e.AccountId).Distinct().ToList();
        var existingAccounts = await db.Accounts
            .Where(a => accountIds.Contains(a.Id))
            .Select(a => a.Id)
            .ToListAsync(ct);

        var missingIds = accountIds.Except(existingAccounts).ToList();
        if (missingIds.Any())
            return Results.BadRequest(new { error = "One or more accounts not found", accountIds = missingIds });

        var now = DateTime.UtcNow;
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            IdempotencyKey = idempotencyKey,
            Description = request.Description?.Trim(),
            CreatedAtUtc = now
        };

        foreach (var entryReq in request.Entries)
        {
            transaction.Entries.Add(new Entry
            {
                Id = Guid.NewGuid(),
                TransactionId = transaction.Id,
                AccountId = entryReq.AccountId,
                Amount = entryReq.Amount,
                Side = entryReq.Side,
                CreatedAtUtc = now
            });
        }

        db.Transactions.Add(transaction);
        await db.SaveChangesAsync(ct);

        return Results.Created($"/transactions/{transaction.Id}", ToResponse(transaction));
    }

    private static TransactionResponse ToResponse(Transaction t)
    {
        return new TransactionResponse(
            t.Id,
            t.IdempotencyKey,
            t.Description,
            t.CreatedAtUtc,
            t.Entries.Select(e => new EntryResponse(e.Id, e.AccountId, e.Amount, e.Side.ToString())).ToList());
    }
}

public record TransactionResponse(
    Guid Id,
    string IdempotencyKey,
    string? Description,
    DateTime CreatedAtUtc,
    List<EntryResponse> Entries);

public record EntryResponse(Guid Id, Guid AccountId, decimal Amount, string Side);
