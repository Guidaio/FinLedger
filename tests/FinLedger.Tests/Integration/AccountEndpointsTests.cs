using System.Net;
using System.Net.Http.Json;
using FinLedger.Api.Contracts;
using FinLedger.Api.Endpoints;
using FinLedger.Domain.Entities;
using FinLedger.Tests.WebApplicationFactory;
using Xunit;

namespace FinLedger.Tests.Integration;

public class AccountEndpointsTests : IClassFixture<FinLedgerWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AccountEndpointsTests(FinLedgerWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateAccount_ValidRequest_Returns201()
    {
        var request = new CreateAccountRequest("Cash", "CASH_" + Guid.NewGuid().ToString("N")[..8], AccountType.Asset);

        var response = await _client.PostAsJsonAsync("/api/v1/accounts", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var account = await response.Content.ReadFromJsonAsync<AccountResponse>();
        Assert.NotNull(account);
        Assert.Equal(request.Name, account.Name);
        Assert.Equal(request.Code, account.Code);
        Assert.Equal("Asset", account.Type);
        Assert.NotEqual(Guid.Empty, account.Id);
    }

    [Fact]
    public async Task CreateAccount_EmptyName_Returns400ProblemDetails()
    {
        var request = new CreateAccountRequest("", "CODE123", AccountType.Asset);

        var response = await _client.PostAsJsonAsync("/api/v1/accounts", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Name and Code are required", content);
    }

    [Fact]
    public async Task CreateAccount_DuplicateCode_Returns409()
    {
        var code = "DUP_" + Guid.NewGuid().ToString("N")[..8];
        var request = new CreateAccountRequest("First", code, AccountType.Asset);

        await _client.PostAsJsonAsync("/api/v1/accounts", request);
        var response = await _client.PostAsJsonAsync("/api/v1/accounts", new CreateAccountRequest("Second", code, AccountType.Liability));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task GetBalance_ExistingAccount_Returns200()
    {
        var createRequest = new CreateAccountRequest("Revenue", "REV_" + Guid.NewGuid().ToString("N")[..8], AccountType.Revenue);
        var createResponse = await _client.PostAsJsonAsync("/api/v1/accounts", createRequest);
        var account = await createResponse.Content.ReadFromJsonAsync<AccountResponse>();
        Assert.NotNull(account);

        var response = await _client.GetAsync($"/api/v1/accounts/{account.Id}/balance");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var balance = await response.Content.ReadFromJsonAsync<BalanceResponse>();
        Assert.NotNull(balance);
        Assert.Equal(account.Id, balance.AccountId);
        Assert.Equal(0, balance.Balance);
    }

    [Fact]
    public async Task GetBalance_NonExistentAccount_Returns404()
    {
        var response = await _client.GetAsync($"/api/v1/accounts/{Guid.NewGuid()}/balance");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
