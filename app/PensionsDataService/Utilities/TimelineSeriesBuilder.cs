using MhpdCommon.Models.MHPDModels;
using MhpdCommon.ViewData;
using PensionsDataService.Models;

namespace PensionsDataService.Utilities;

public class TimelineSeriesBuilder(ITimelineArrangementFactory arrangementFactory) : ITimelineSeriesBuilder
{
    public TimelineSeries Build(IEnumerable<RetrievedPensionRecord> pensions)
    {
        var series = new TimelineSeries();

        var allArrangements = new List<TimelineArrangement>();

        var changeYears = new HashSet<int>();

        foreach (var pension in pensions)
        {
            var arrangement = arrangementFactory.Create(pension);

            if (arrangement != null)
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
            var timelineYear = new TimelineYear { Year = year };

            foreach (var arrangement in allArrangements.Where(a => a.IsActiveInYear(year)))
            {
                timelineYear.AddArrangement(arrangement);
            }

            series.Years.Add(timelineYear);
        }

        PopulateKeys(series, allArrangements);

        return series;
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
