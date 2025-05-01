using MhpdCommon.Constants;
using System.ComponentModel.DataAnnotations;

namespace PensionsDataService.Models;

public class PensionsDataRetrievalRequest
{
    [Required]
    [RegularExpression(ApiConstants.UrlPattern)]
    public string Ticket { get; set; } = string.Empty;

    [Required]
    [MinLength(1)]
    public string ClientId { get; set; } = string.Empty;
}