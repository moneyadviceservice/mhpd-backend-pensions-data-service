using MhpdCommon.Constants;
using System.ComponentModel.DataAnnotations;

namespace MhpdCommon.Models.MessageBodyModels;

public class PensionsDataRequestModel
{
    [Required]
    [MinLength(1)]
    public string? ClientId { get; set; }

    [Required]
    [MinLength(1)]
    public string? ClientSecret { get; set; }

    [Required]
    [MinLength(1)]
    public string? AuthorisationCode { get; set; }

    [Required]
    [MinLength(1)]
    [RegularExpression(ApiConstants.UrlPattern)]
    public string? RedirectUrl { get; set; }

    [Required]
    [MinLength(43)]
    [MaxLength(128)]
    [RegularExpression(ApiConstants.CodeVerifierPattern)]
    public string? CodeVerifier { get; set; }
}