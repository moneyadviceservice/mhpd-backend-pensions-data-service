using System.ComponentModel.DataAnnotations;

namespace MaPSCDAService.Configuration;

public class UriSettings
{
    public const string TargetUrlVariable = nameof(TargetUrlVariable);

    [Required(ErrorMessage = $"{TargetUrlVariable} is required.")]
    public string? RedirectTargetUrl { get; set; }
}