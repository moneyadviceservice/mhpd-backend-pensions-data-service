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
        int index = Arrangements.FindIndex(a =>
            a.StartYear > arrangement.StartYear);

        if (index < 0)
        {
            Arrangements.Add(arrangement);
        }
        else
        {
            Arrangements.Insert(index, arrangement);
        }
    }
}
