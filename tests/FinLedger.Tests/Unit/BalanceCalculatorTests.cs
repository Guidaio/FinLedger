using FinLedger.Domain.Entities;
using FinLedger.Domain.Services;
using Xunit;

namespace FinLedger.Tests.Unit;

public class BalanceCalculatorTests
{
    [Theory]
    [InlineData(AccountType.Asset, 100, 0, 100)]
    [InlineData(AccountType.Asset, 100, 30, 70)]
    [InlineData(AccountType.Asset, 0, 50, -50)]
    [InlineData(AccountType.Expense, 200, 50, 150)]
    public void Calculate_AssetOrExpense_DebitsMinusCredits(AccountType type, decimal debits, decimal credits, decimal expected)
    {
        var result = BalanceCalculator.Calculate(type, debits, credits);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(AccountType.Liability, 0, 100, 100)]
    [InlineData(AccountType.Liability, 30, 100, 70)]
    [InlineData(AccountType.Equity, 50, 200, 150)]
    [InlineData(AccountType.Revenue, 100, 250, 150)]
    public void Calculate_LiabilityEquityRevenue_CreditsMinusDebits(AccountType type, decimal debits, decimal credits, decimal expected)
    {
        var result = BalanceCalculator.Calculate(type, debits, credits);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Calculate_ZeroEntries_ReturnsZero()
    {
        Assert.Equal(0, BalanceCalculator.Calculate(AccountType.Asset, 0, 0));
        Assert.Equal(0, BalanceCalculator.Calculate(AccountType.Revenue, 0, 0));
    }
}
