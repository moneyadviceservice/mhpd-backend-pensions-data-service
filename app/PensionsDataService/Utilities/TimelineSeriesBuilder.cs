using MhpdCommon.Models.MHPDModels;
using PensionsDataService.Models;

namespace PensionsDataService.Utilities;

public class TimelineSeriesBuilder(ITimelineArrangementFactory arrangementFactory) : ITimelineSeriesBuilder
{
    public TimelineSeries Build(IEnumerable<RetrievedPensionRecord> pensions, bool includeWithoutIncome)
    {
        return Build(pensions, includeWithoutIncome, arrangementFactory.CreateAll);
    }

    public TimelineSeries BuildLegacy(IEnumerable<RetrievedPensionRecord> pensions, bool includeWithoutIncome)
    {
        return Build(pensions, includeWithoutIncome, arrangementFactory.CreateAllLegacy);
    }

    public TimelineSeries BuildAlternative(IEnumerable<RetrievedPensionRecord> pensions, bool includeWithoutIncome)
    {
        return Build(pensions, includeWithoutIncome, arrangementFactory.CreateAllAlternative);
    }

    private static TimelineSeries Build(IEnumerable<RetrievedPensionRecord> pensions, bool includeWithoutIncome,
        Func<RetrievedPensionRecord, bool, IEnumerable<TimelineArrangement>> arrangementSelector)
    {
        var series = new TimelineSeries();
        var allArrangements = new List<TimelineArrangement>();
        var changeYears = new HashSet<int>();

        foreach (var pension in pensions)
        {
            var pensionArrangements = arrangementSelector(pension, includeWithoutIncome);
            foreach (var arrangement in pensionArrangements)
            {
                allArrangements.Add(arrangement);
                changeYears.Add(arrangement.StartYear);

                if (arrangement.HasEndYear)
                {
                    changeYears.Add(arrangement.EndYear!.Value + 1);
                }
                if (arrangement.HasLumpSum)
                {
                    changeYears.Add(arrangement.LumpSumYear!.Value);
                }
            }
        }

        foreach (var year in changeYears.OrderBy(y => y))
        {
            var timelineYear = AggregateArramgements(allArrangements, year);
            series.Years.Add(timelineYear);
        }

        PopulateKeys(series, allArrangements);

        return series;
    }

    private static TimelineYear AggregateArramgements(List<TimelineArrangement> allArrangements, int year)
    {
        var timelineYear = new TimelineYear { Year = year };

        var activeArrangements = allArrangements
                    .Where(a => a.IsActiveInYear(year))
                    .GroupBy(a => a.Id);

        foreach (var pensionGroup in activeArrangements)
        {
            var first = pensionGroup.OrderBy(a => a.PayableDate).First();

            var aggregated = new TimelineArrangement
            {
                Id = first.Id,
                SchemeName = first.SchemeName,
                PensionType = first.PensionType,
                PayableDate = first.PayableDate
            };

            if (pensionGroup.Any(a => a.HasRecurringIncome))
            {
                aggregated.MonthlyAmount = pensionGroup.Sum(a => a.MonthlyAmount);
                aggregated.AnnualAmount = pensionGroup.Sum(a => a.AnnualAmount);
            }

            if (pensionGroup.Any(a => a.HasLumpSum))
            {
                aggregated.LumpSumAmount = pensionGroup.Sum(a => a.LumpSumAmount);
                aggregated.LumpSumYear = year;
            }

            timelineYear.AddArrangement(aggregated);
        }

        return timelineYear;
    }

    private static void PopulateKeys(TimelineSeries series, List<TimelineArrangement> arrangements)
    {
        var pensionTypes = arrangements
            .Select(a => a.PensionType)
            .Distinct()
            .ToList();

        var hasLumpSum = arrangements.Any(a => a.LumpSumAmount.HasValue);

        series.Keys.AddRange(pensionTypes);

        if (hasLumpSum)
        {
            series.Keys.Add(Constants.PensionTypes.LU);
        }
    }
}
