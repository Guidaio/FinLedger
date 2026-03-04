using System.Net;
using System.Net.Http.Json;
using FinLedger.Api.Contracts;
using FinLedger.Api.Endpoints;
using FinLedger.Domain.Entities;
using FinLedger.Tests.WebApplicationFactory;
using Xunit;

namespace FinLedger.Tests.Integration;

public class TransactionEndpointsTests : IClassFixture<FinLedgerWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TransactionEndpointsTests(FinLedgerWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateTransaction_ValidBalancedEntries_Returns201()
    {
        var (cashId, revId) = await CreateTwoAccountsAsync();
        var idempotencyKey = "tx-" + Guid.NewGuid().ToString("N");
        var request = new CreateTransactionRequest(idempotencyKey, "Sale", new List<EntryRequest>
        {
            new(cashId, 100, EntrySide.Debit),
            new(revId, 100, EntrySide.Credit)
        });

        using var req = new HttpRequestMessage(HttpMethod.Post, "/api/v1/transactions");
        req.Headers.Add("Idempotency-Key", idempotencyKey);
        req.Content = JsonContent.Create(request);
        var response = await _client.SendAsync(req);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var tx = await response.Content.ReadFromJsonAsync<TransactionResponse>();
        Assert.NotNull(tx);
        Assert.Equal(idempotencyKey, tx.IdempotencyKey);
        Assert.Equal(2, tx.Entries.Count);
    }

    [Fact]
    public async Task CreateTransaction_WithIdempotencyHeader_Returns201Then200()
    {
        var (cashId, revId) = await CreateTwoAccountsAsync();
        var idempotencyKey = "tx-idem-" + Guid.NewGuid().ToString("N");
        var request = new CreateTransactionRequest(null, "Sale", new List<EntryRequest>
        {
            new(cashId, 50, EntrySide.Debit),
            new(revId, 50, EntrySide.Credit)
        });

        using var req1 = new HttpRequestMessage(HttpMethod.Post, "/api/v1/transactions");
        req1.Headers.Add("Idempotency-Key", idempotencyKey);
        req1.Content = JsonContent.Create(request);

        var response1 = await _client.SendAsync(req1);
        Assert.Equal(HttpStatusCode.Created, response1.StatusCode);

        using var req2 = new HttpRequestMessage(HttpMethod.Post, "/api/v1/transactions");
        req2.Headers.Add("Idempotency-Key", idempotencyKey);
        req2.Content = JsonContent.Create(request);

        var response2 = await _client.SendAsync(req2);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);

        var tx1 = await response1.Content.ReadFromJsonAsync<TransactionResponse>();
        var tx2 = await response2.Content.ReadFromJsonAsync<TransactionResponse>();
        Assert.Equal(tx1!.Id, tx2!.Id);
    }

    [Fact]
    public async Task CreateTransaction_UnbalancedEntries_Returns422()
    {
        var (cashId, revId) = await CreateTwoAccountsAsync();
        var request = new CreateTransactionRequest("tx-unbal", null, new List<EntryRequest>
        {
            new(cashId, 100, EntrySide.Debit),
            new(revId, 50, EntrySide.Credit) // unbalanced
        });

        var response = await _client.PostAsJsonAsync("/api/v1/transactions", request);

        Assert.Equal((HttpStatusCode)422, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Entries must balance", content);
    }

    [Fact]
    public async Task CreateTransaction_MissingIdempotencyKey_Returns400()
    {
        var request = new CreateTransactionRequest(null, null, new List<EntryRequest>
        {
            new(Guid.NewGuid(), 100, EntrySide.Debit),
            new(Guid.NewGuid(), 100, EntrySide.Credit)
        });

        var response = await _client.PostAsJsonAsync("/api/v1/transactions", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<(Guid CashId, Guid RevId)> CreateTwoAccountsAsync()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var cashRes = await _client.PostAsJsonAsync("/api/v1/accounts",
            new CreateAccountRequest("Cash", "CASH_" + suffix, AccountType.Asset));
        var revRes = await _client.PostAsJsonAsync("/api/v1/accounts",
            new CreateAccountRequest("Revenue", "REV_" + suffix, AccountType.Revenue));

        var cash = await cashRes.Content.ReadFromJsonAsync<AccountResponse>();
        var rev = await revRes.Content.ReadFromJsonAsync<AccountResponse>();
        return (cash!.Id, rev!.Id);
    }
}
