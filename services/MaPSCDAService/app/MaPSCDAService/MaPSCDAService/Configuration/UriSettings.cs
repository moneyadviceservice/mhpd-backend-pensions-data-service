using System.ComponentModel.DataAnnotations;

namespace MaPSCDAService;

public class UriSettings
{
    public const string TargetUrlVariable = nameof(TargetUrlVariable);

    [Required(ErrorMessage = $"{TargetUrlVariable} is required.")]
    public string? RedirectTargetUrl { get; set; }
}
