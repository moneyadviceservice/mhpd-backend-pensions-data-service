using System.ComponentModel.DataAnnotations;

namespace RetrievedPensionsRecordFunction.Models.Configuration;

public class MhpdCosmosConfiguration
{
    public const string DatabaseVariable = nameof(DatabaseId);
    public const string ContainerVariable = nameof(ContainerId);
    public const string PartitionVariable = nameof(ContainerPartitionKey);

    [Required(ErrorMessage = $"{DatabaseVariable} is required")]
    public string? DatabaseId { get; set; }
    [Required(ErrorMessage = $"{ContainerVariable} is required")]
    public string? ContainerId { get; set; }
    [Required(ErrorMessage = $"{PartitionVariable} is required")]
    public string? ContainerPartitionKey { get; set; }
}
