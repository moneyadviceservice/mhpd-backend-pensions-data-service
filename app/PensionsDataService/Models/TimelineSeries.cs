namespace PensionsDataService.Models;

public class TimelineSeries
{
    public bool IsPensionRetrievalComplete { get; set; }

    public List<string> Keys { get; } = [];

    public List<TimelineYear> Years { get; } = [];
}
