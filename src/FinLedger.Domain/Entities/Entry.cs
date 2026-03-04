namespace FinLedger.Domain.Entities;

/// <summary>
/// A single ledger entry (debit or credit) linked to a transaction and an account.
/// Double-entry rule: for each transaction, sum(debits) = sum(credits).
/// </summary>
public class Entry
{
    public Guid Id { get; set; }
    public Guid TransactionId { get; set; }
    public Guid AccountId { get; set; }
    public decimal Amount { get; set; }
    public EntrySide Side { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public Transaction Transaction { get; set; } = null!;
    public Account Account { get; set; } = null!;
}

/// <summary>
/// Debit or credit side of a ledger entry.
/// </summary>
public enum EntrySide
{
    Debit,
    Credit
}
