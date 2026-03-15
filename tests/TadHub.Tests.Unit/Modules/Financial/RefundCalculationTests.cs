namespace TadHub.Tests.Unit.Modules.Financial;

/// <summary>
/// Tests the refund calculation formulas used by RefundCalculationService.
/// The actual service relies on raw SQL for cross-module data, so we test the
/// core amortization logic directly: ValuePerMonth = TotalPaid / ContractMonths,
/// RefundAmount = Max(0, TotalPaid - MonthsWorked * ValuePerMonth).
/// </summary>
public class RefundCalculationTests
{
    #region Amortization formula

    [Fact]
    public void AmortizationFormula_StandardCase_CalculatesCorrectly()
    {
        // 24-month contract, 24000 AED paid, 6 months worked
        decimal totalPaid = 24000m;
        int contractMonths = 24;
        decimal monthsWorked = 6m;

        var valuePerMonth = totalPaid / contractMonths; // 1000
        var refundAmount = Math.Max(0, totalPaid - (monthsWorked * valuePerMonth));

        valuePerMonth.Should().Be(1000m);
        refundAmount.Should().Be(18000m);
    }

    [Fact]
    public void AmortizationFormula_ZeroMonthsWorked_RefundsFullAmount()
    {
        decimal totalPaid = 24000m;
        int contractMonths = 24;
        decimal monthsWorked = 0m;

        var valuePerMonth = totalPaid / contractMonths;
        var refundAmount = Math.Max(0, totalPaid - (monthsWorked * valuePerMonth));

        refundAmount.Should().Be(24000m);
    }

    [Fact]
    public void AmortizationFormula_FullContractCompleted_RefundsZero()
    {
        decimal totalPaid = 24000m;
        int contractMonths = 24;
        decimal monthsWorked = 24m;

        var valuePerMonth = totalPaid / contractMonths;
        var refundAmount = Math.Max(0, totalPaid - (monthsWorked * valuePerMonth));

        refundAmount.Should().Be(0m);
    }

    [Fact]
    public void AmortizationFormula_MoreThanContractMonths_ClampsToZero()
    {
        // Worker worked longer than contract — no negative refund
        decimal totalPaid = 24000m;
        int contractMonths = 24;
        decimal monthsWorked = 30m;

        var valuePerMonth = totalPaid / contractMonths;
        var refundAmount = Math.Max(0, totalPaid - (monthsWorked * valuePerMonth));

        refundAmount.Should().Be(0m);
    }

    [Fact]
    public void AmortizationFormula_HalfContract_RefundsHalf()
    {
        decimal totalPaid = 48000m;
        int contractMonths = 24;
        decimal monthsWorked = 12m;

        var valuePerMonth = totalPaid / contractMonths; // 2000
        var refundAmount = Math.Max(0, totalPaid - (monthsWorked * valuePerMonth));

        valuePerMonth.Should().Be(2000m);
        refundAmount.Should().Be(24000m);
    }

    #endregion

    #region RoundDown partial month method

    [Fact]
    public void RoundDown_CountsOnlyFullMonths()
    {
        var startDate = new DateOnly(2025, 1, 1);
        var returnDate = new DateOnly(2025, 7, 15); // 6 months + 15 days

        var monthsWorked = CalculateMonthsWorkedRoundDown(startDate, returnDate);

        monthsWorked.Should().Be(6m); // 15 extra days ignored
    }

    [Fact]
    public void RoundDown_ExactMonthBoundary_CountsFullMonth()
    {
        var startDate = new DateOnly(2025, 1, 1);
        var returnDate = new DateOnly(2025, 4, 1); // Exactly 3 months

        var monthsWorked = CalculateMonthsWorkedRoundDown(startDate, returnDate);

        monthsWorked.Should().Be(3m);
    }

    [Fact]
    public void RoundDown_LessThanOneMonth_ReturnsZero()
    {
        var startDate = new DateOnly(2025, 1, 1);
        var returnDate = new DateOnly(2025, 1, 25); // 24 days

        var monthsWorked = CalculateMonthsWorkedRoundDown(startDate, returnDate);

        monthsWorked.Should().Be(0m);
    }

    [Fact]
    public void RoundDown_SameDay_ReturnsZero()
    {
        var startDate = new DateOnly(2025, 3, 15);
        var returnDate = new DateOnly(2025, 3, 15);

        var monthsWorked = CalculateMonthsWorkedRoundDown(startDate, returnDate);

        monthsWorked.Should().Be(0m);
    }

    #endregion

    #region ProRata partial month method

    [Fact]
    public void ProRata_IncludesPartialMonthFraction()
    {
        var startDate = new DateOnly(2025, 1, 1);
        var returnDate = new DateOnly(2025, 7, 15); // 6 months + 15 days

        var monthsWorked = CalculateMonthsWorkedProRata(startDate, returnDate);

        // 6 full months + 15/31 partial (July has 31 days from Jul 1 to Aug 1)
        monthsWorked.Should().BeGreaterThan(6m);
        monthsWorked.Should().BeLessThan(7m);
    }

    [Fact]
    public void ProRata_ExactMonthBoundary_ReturnsWholeNumber()
    {
        var startDate = new DateOnly(2025, 1, 1);
        var returnDate = new DateOnly(2025, 4, 1); // Exactly 3 months

        var monthsWorked = CalculateMonthsWorkedProRata(startDate, returnDate);

        monthsWorked.Should().Be(3m);
    }

    [Fact]
    public void ProRata_LessThanOneMonth_ReturnsFraction()
    {
        var startDate = new DateOnly(2025, 1, 1);
        var returnDate = new DateOnly(2025, 1, 16); // 15 days

        var monthsWorked = CalculateMonthsWorkedProRata(startDate, returnDate);

        // 0 full months + 15/31 partial (Jan has 31 days)
        monthsWorked.Should().BeGreaterThan(0m);
        monthsWorked.Should().BeLessThan(1m);
    }

    [Fact]
    public void ProRata_SameDay_ReturnsZero()
    {
        var startDate = new DateOnly(2025, 3, 15);
        var returnDate = new DateOnly(2025, 3, 15);

        var monthsWorked = CalculateMonthsWorkedProRata(startDate, returnDate);

        monthsWorked.Should().Be(0m);
    }

    #endregion

    #region End-to-end formula with partial months

    [Fact]
    public void FullCalculation_RoundDown_IgnoresPartialMonth()
    {
        decimal totalPaid = 24000m;
        int contractMonths = 24;
        var startDate = new DateOnly(2025, 1, 1);
        var returnDate = new DateOnly(2025, 7, 15); // 6 months + 15 days

        var monthsWorked = CalculateMonthsWorkedRoundDown(startDate, returnDate);
        var valuePerMonth = Math.Round(totalPaid / contractMonths, 2);
        var refundAmount = Math.Round(Math.Max(0, totalPaid - (monthsWorked * valuePerMonth)), 2);

        monthsWorked.Should().Be(6m);
        valuePerMonth.Should().Be(1000m);
        refundAmount.Should().Be(18000m); // 24000 - 6*1000
    }

    [Fact]
    public void FullCalculation_ProRata_IncludesPartialMonth()
    {
        decimal totalPaid = 24000m;
        int contractMonths = 24;
        var startDate = new DateOnly(2025, 1, 1);
        var returnDate = new DateOnly(2025, 7, 15);

        var monthsWorked = CalculateMonthsWorkedProRata(startDate, returnDate);
        monthsWorked = Math.Round(monthsWorked, 2);
        var valuePerMonth = Math.Round(totalPaid / contractMonths, 2);
        var refundAmount = Math.Round(Math.Max(0, totalPaid - (monthsWorked * valuePerMonth)), 2);

        // ProRata includes partial month, so refund is less than RoundDown
        refundAmount.Should().BeLessThan(18000m);
        refundAmount.Should().BeGreaterThan(17000m);
    }

    #endregion

    #region Helpers (mirror RefundCalculationService logic)

    /// <summary>
    /// RoundDown: count only full months (same algorithm as RefundCalculationService).
    /// </summary>
    private static decimal CalculateMonthsWorkedRoundDown(DateOnly startDate, DateOnly returnDate)
    {
        var fullMonths = 0;
        var cursor = startDate;
        while (cursor.AddMonths(1) <= returnDate)
        {
            fullMonths++;
            cursor = startDate.AddMonths(fullMonths);
        }
        return fullMonths;
    }

    /// <summary>
    /// ProRata: full months + fractional partial month (same algorithm as RefundCalculationService).
    /// </summary>
    private static decimal CalculateMonthsWorkedProRata(DateOnly startDate, DateOnly returnDate)
    {
        var fullMonths = 0;
        var cursor = startDate;
        while (cursor.AddMonths(1) <= returnDate)
        {
            fullMonths++;
            cursor = startDate.AddMonths(fullMonths);
        }
        var nextMonth = startDate.AddMonths(fullMonths + 1);
        var daysInPartialMonth = nextMonth.DayNumber - cursor.DayNumber;
        var remainingDays = returnDate.DayNumber - cursor.DayNumber;
        var partialFraction = daysInPartialMonth > 0 ? (decimal)remainingDays / daysInPartialMonth : 0;
        return fullMonths + partialFraction;
    }

    #endregion
}
