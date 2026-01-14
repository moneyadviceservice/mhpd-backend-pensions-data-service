namespace PensionsDataService.Utilities;

public class PensionServiceUtilities(IPensionAnonymizer pensionAnonymizer,
    ICardDataRuleEngine cardDataRuleEngine, ISummaryDataRuleEngine summaryDataRuleEngine)
{
    public IPensionAnonymizer Anonymizer { get; } = pensionAnonymizer;
    public ICardDataRuleEngine CardDataRuleEngine { get; } = cardDataRuleEngine;
    public ISummaryDataRuleEngine SummaryDataRuleEngine { get; } = summaryDataRuleEngine;
}
