using MhpdCommon.Models.RequestHeaderModel;
using Microsoft.AspNetCore.Mvc;

namespace PensionsDataService.HttpClients;

public interface IRetrievalRecordFunctionClient
{
    Task<IActionResult> GetAsync(RequestHeaderModel requestHeader);
}