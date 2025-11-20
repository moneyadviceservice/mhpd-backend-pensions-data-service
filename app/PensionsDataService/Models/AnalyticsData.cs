using MhpdCommon.Models.MHPDModels;
using static MhpdCommon.ViewData.EvaluationConstants;

namespace PensionsDataService.Models;

public class AnalyticsData
{
    public int TotalErrorPensions { get; set; }

    public int TotalPensions { get; set; }

    public int TotalUnsupportedPensions { get; set; }

    public List<dynamic> IncompletePensions { get; set; } = [];

    public List<dynamic> ConfirmedPensions { get; set; } = [];

    public List<dynamic> UnconfirmedPensions { get; set; } = [];

    public List<dynamic> UnsupportedPensions { get; set; } = [];

    public List<dynamic> ErroredPensions { get; set; } = [];

    public void SplitPensionsByStatus(IEnumerable<RetrievedPensionRecord> pensions, IEnumerable<PeiDataModel> peis)
    {
        foreach (var pension in pensions)
        {
            switch (pension.Category)
            {
                case Category.Pending:
                    IncompletePensions.Add(pension.RetrievalResult);
                    TotalPensions++;
                    break;
                case Category.Confirmed:
                    ConfirmedPensions.Add(pension.RetrievalResult);
                    TotalPensions++;
                    break;
                case Category.Contact:
                    UnconfirmedPensions.Add(pension.RetrievalResult);
                    TotalPensions++;
                    break;
                case Category.Unsupported:
                    UnsupportedPensions.Add(pension.RetrievalResult);
                    TotalUnsupportedPensions++;
                    break;
                default:
                    var peiInfo = peis.Single(p => p.Pei == pension.Pei);
                    ErroredPensions.Add(peiInfo);
                    TotalErrorPensions++;
                    break;
            }
        }
    }
}
