namespace PensionsDataService.Utilities;

public class PensionServiceUtilities(IPensionAnonymizer pensionAnonymizer,
    ICardDataRuleEngine cardDataRuleEngine, ISummaryDataRuleEngine summaryDataRuleEngine,
    ITimelineSeriesBuilder timelineSeriesBuilder)
{
    public IPensionAnonymizer Anonymizer { get; } = pensionAnonymizer;
    public ICardDataRuleEngine CardDataRuleEngine { get; } = cardDataRuleEngine;
    public ISummaryDataRuleEngine SummaryDataRuleEngine { get; } = summaryDataRuleEngine;
    public ITimelineSeriesBuilder TimelineSeriesBuilder { get; } = timelineSeriesBuilder;
}
