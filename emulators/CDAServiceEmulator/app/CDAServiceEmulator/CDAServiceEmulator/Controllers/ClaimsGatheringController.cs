using MhpdCommon.Constants;
using MhpdCommon.Extensions;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Utils;
using Microsoft.AspNetCore.Mvc;

namespace CDAServiceEmulator.Controllers
{
    [Route("/")]
    [ApiController]
    public class ClaimsGatheringController(ILogger<ClaimsGatheringController> logger) : ControllerBase
    {
        [HttpGet]
        [Route("claims_gathering")]
        public IActionResult CollectClaims([FromQuery] ClaimsGatheringRequestModel request)
        {
            logger.LogRequest(request);

            var result = request.IsValidClaimsGatheringRequest();
            if (!result.IsValid)
            {
                return BadRequest(result.ErrorMessage);
            }

            var response = new
            {
                ticket = SecurityConstants.Jwe.AuthorizedPermissionTicket
            };

            logger.LogResponse(response);

            return Redirect(UrlHelper.ConstructEndPoint(response, request.ClaimsRedirectUri!));
        }
    }
}
