using FinLedger.Api.Contracts;
using FinLedger.Api.ProblemDetails;
using FinLedger.Domain.Entities;
using FinLedger.Domain.Services;
using FinLedger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinLedger.Api.Endpoints;

public static class AccountEndpoints
{
    public static void MapAccountEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/accounts")
            .WithTags("Accounts")
            .WithOpenApi();

        group.MapPost("/", CreateAccount)
            .WithName("CreateAccount")
            .WithOpenApi()
            .Produces<AccountResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status409Conflict);

        group.MapGet("/{id:guid}/balance", GetBalance)
            .WithName("GetAccountBalance")
            .WithOpenApi()
            .Produces<BalanceResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> CreateAccount(
        CreateAccountRequest request,
        LedgerDbContext db,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Code))
            return ProblemDetailsExtensions.BadRequest("Name and Code are required");

        var exists = await db.Accounts.AnyAsync(a => a.Code == request.Code, ct);
        if (exists)
            return ProblemDetailsExtensions.Conflict($"Account with code '{request.Code}' already exists");

        var account = new Account
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Code = request.Code.Trim(),
            Type = request.Type,
            CreatedAtUtc = DateTime.UtcNow
        };

        db.Accounts.Add(account);
        await db.SaveChangesAsync(ct);

        return Results.Created($"/api/v1/accounts/{account.Id}", new AccountResponse(
            account.Id,
            account.Name,
            account.Code,
            account.Type.ToString(),
            account.CreatedAtUtc));
    }

    private static async Task<IResult> GetBalance(
        Guid id,
        LedgerDbContext db,
        CancellationToken ct)
    {
        var account = await db.Accounts.FindAsync([id], ct);
        if (account is null)
            return ProblemDetailsExtensions.NotFound("Account not found");

        var entries = await db.Entries
            .Where(e => e.AccountId == id)
            .Select(e => new { e.Amount, e.Side })
            .ToListAsync(ct);

        var totalDebits = entries.Where(e => e.Side == EntrySide.Debit).Sum(e => e.Amount);
        var totalCredits = entries.Where(e => e.Side == EntrySide.Credit).Sum(e => e.Amount);
        var balance = BalanceCalculator.Calculate(account.Type, totalDebits, totalCredits);

        return Results.Ok(new BalanceResponse(account.Id, account.Code, balance));
    }
}

public record AccountResponse(Guid Id, string Name, string Code, string Type, DateTime CreatedAtUtc);
public record BalanceResponse(Guid AccountId, string AccountCode, decimal Balance);
