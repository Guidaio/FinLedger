using FinLedger.Domain.Entities;

namespace FinLedger.Domain.Services;

/// <summary>
/// Calculates account balance from ledger entries.
/// Asset/Expense: debit increases, credit decreases.
/// Liability/Equity/Revenue: credit increases, debit decreases.
/// </summary>
public static class BalanceCalculator
{
    public static decimal Calculate(
        AccountType accountType,
        decimal totalDebits,
        decimal totalCredits)
    {
        return accountType is AccountType.Asset or AccountType.Expense
            ? totalDebits - totalCredits
            : totalCredits - totalDebits;
    }
}
