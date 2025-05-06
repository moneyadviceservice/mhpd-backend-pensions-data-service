using System.ComponentModel.DataAnnotations;

namespace MhpdCommon.Models.MHPDModels;

public class PeiRetrievalDetailsResponseModel
{
    [MinLength(1)]
    public string? PeisId { get; set; }
}
