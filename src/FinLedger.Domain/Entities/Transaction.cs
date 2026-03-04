namespace FinLedger.Domain.Entities;

/// <summary>
/// Represents a financial transaction. Must have balanced entries (sum of debits = sum of credits).
/// </summary>
public class Transaction
{
    public Guid Id { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public ICollection<Entry> Entries { get; set; } = new List<Entry>();
}
