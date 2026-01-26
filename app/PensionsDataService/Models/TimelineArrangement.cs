namespace PensionsDataService.Models;

public class TimelineArrangement
{
    public DateTime PayableDate { get; init; }
    public int StartYear => PayableDate.Year;
    public int? EndYear { get; init; }
    public int? LumpSumYear { get; init; }
    public string Id { get; init; } = string.Empty;
    public string SchemeName { get; init; } = string.Empty;
    public string PensionType { get; init; } = string.Empty;
    public decimal? MonthlyAmount { get; init; }
    public decimal? AnnualAmount { get; init; }
    public decimal? LumpSumAmount { get; init; }

    public bool IsActiveInYear(int year)
    {
        if (HasLumpSum && year == LumpSumYear)
        {
            return true;
        }

        if (HasRecurringIncome)
        {
            return CanContributeToYear(year);
        }

        return false;
    }

    public bool CanContributeToYear(int year)
    {
        if (year < StartYear)
        {
            return false;
        }

        if ((HasEndYear && year > EndYear))
        {
            return false;
        }

        return true;
    }

    internal bool HasEndYear => EndYear.HasValue;

    internal bool HasRecurringIncome => MonthlyAmount.HasValue && AnnualAmount.HasValue;

    internal bool HasLumpSum => LumpSumAmount.HasValue && LumpSumYear.HasValue;
}
