namespace FinLedger.Domain.Entities;

/// <summary>
/// Represents a ledger account (e.g. merchant, cash, bank).
/// </summary>
public class Account
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public AccountType Type { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public ICollection<Entry> Entries { get; set; } = new List<Entry>();
}

/// <summary>
/// Standard account types for double-entry bookkeeping.
/// </summary>
public enum AccountType
{
    Asset,
    Liability,
    Equity,
    Revenue,
    Expense
}
