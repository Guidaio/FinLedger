using FinLedger.Domain.Entities;

namespace FinLedger.Api.Contracts;

public record CreateAccountRequest(string Name, string Code, AccountType Type);
