namespace PensionsDataService.Models;

public class TimelineYear
{
    public int Year { get; init; }
    public decimal MonthlyTotal { get; private set; }
    public decimal AnnualTotal { get; private set; }
    public List<TimelineArrangement> Arrangements { get; set; } = [];

    public void AddArrangement(TimelineArrangement arrangement)
    {
        InsertInPayableDateOrder(arrangement);

        if (arrangement.CanContributeToYear(Year))
        {
            MonthlyTotal += arrangement.MonthlyAmount ?? 0m;
            AnnualTotal += arrangement.AnnualAmount ?? 0m;
        }
    }

    private void InsertInPayableDateOrder(TimelineArrangement arrangement)
    {
        int index = Arrangements.FindIndex(existing =>
            CompareArrangements(existing, arrangement) > 0);

        if (index < 0)
        {
            Arrangements.Add(arrangement);
        }
        else
        {
            Arrangements.Insert(index, arrangement);
        }
    }

    private static int CompareArrangements(
    TimelineArrangement first,
    TimelineArrangement second)
    {
        // Primary: Start year
        int result = first.PayableDate.CompareTo(second.PayableDate);
        if (result != 0)
        {
            return result;
        }

        // Secondary: Scheme name (case insensitive)
        result = string.Compare(
            first.SchemeName,
            second.SchemeName,
            StringComparison.OrdinalIgnoreCase);

        if (result != 0)
        {
            return result;
        }

        // Tertiary: Monthly amount
        return (first.MonthlyAmount ?? 0m).CompareTo(second.MonthlyAmount ?? 0m);
    }
}
