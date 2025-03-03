using MhpdCommon.Constants.HttpClient;
using MhpdCommon.Utils;

namespace MhpdCommon.Models.Configuration;

public class CommonHttpConfiguration
{
    public string? CdaServiceUrl { get; set; }
    public string? PdpServiceUrl { get; set; }
    public string? MapsCdaServiceUrl { get; set; }
    public string? TokenIntegrationServiceUrl { get; set; }
    public string? RetrievedPensionsServiceUrl { get; set; }
    public string? PensionRetrievalServiceUrl { get; set; }
    public string? PeiIntegrationServiceUrl { get; set; }
    public string? ViewDataIntegrationServiceUrl { get; set; }

    public string CdaTokenEndpoint => UrlHelper.ConstructPath(CdaServiceUrl, HttpEndpoints.External.CdaTokenServiceEndpoint);
}
