using CDAServiceEmulator.CosmosRepository;
using CDAServiceEmulator.Models.Peis;
using CDAServiceEmulator.Models.Token;
using CDAServiceEmulator.Models.ViewData;
using MhpdCommon.Utils;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;

namespace CDAServiceEmulator.Controllers;

[Route("/")]
[ApiController]
[ExcludeFromCodeCoverage]
public class ScenarioController(ILogger<ScenarioController> logger, 
    TokenEmulatorPiesIdScenarioModelsRepository scenarioModelsRepository,
    CdaPeisEmulatorScenarioModelRepository peiModelRepository,
    ViewDataRepository viewDataRepository,
    IViewDataTransformer dataTransformer,
    IMessageParser messageParser,
    IIdValidator idValidator) : ControllerBase
{
    private const string HolderNameConfigurationId = "ca6f57a9-51de-4e9f-9a5a-d3fddcccd029";
    private readonly JsonSerializerOptions serializerOptions = new() { WriteIndented = true };

    [HttpGet]
    [Route("scenarios/{scenarioCode}")]
    public async Task<IActionResult> GetAsync(string scenarioCode)
    {
        var scenario = await scenarioModelsRepository.GetByIdAsync(scenarioCode, scenarioCode);

        if(scenario == null) return NotFound();

        var peiModel = await peiModelRepository.GetByIdAsync(scenario.PeisIdStartCode!, scenario.PeisIdStartCode!);

        if(peiModel == null) return NotFound();

        var peis = peiModel.DataPoints!.SelectMany(point => point.ResponsePayload!.PeiList!).DistinctBy(pei => pei.Pei);

        var arrangements = new List<JsonElement>();

        foreach(var pei in peis)
        {
            if(idValidator.TryExtractPei(pei.Pei, out _, out var assetId))
            {
                var viewData = await viewDataRepository.GetByIdAsync(assetId, assetId);

                if(viewData == null) continue;

                var assetPayload = viewData.ViewData!.ToString();

                var transformPayload = TransformViewData(assetId, assetPayload, pei.Pei!);
                var element = GetArrangement(transformPayload);
                arrangements.Add(element);
            }
        }

        string policyString = CreatePolicyContent(arrangements, scenarioCode);

        return Ok(policyString);
    }

    [HttpGet]
    [Route("scenarios")]
    public async Task<IActionResult> GetAllAsync()
    {
        var scenarios = await scenarioModelsRepository.GetAllAsync();
        var validScenarios = scenarios.Where(scenario => !scenario.IsDebugScenario).ToList();

        return Ok(validScenarios);
    }

    [HttpPost]
    [Route("scenarios")]
    public async Task<IActionResult> PostAsync([FromBody] JsonElement payload, [FromHeader] string scenarioCode)
    {
        var assetPayload = payload.GetRawText();

        if (string.IsNullOrWhiteSpace(assetPayload))
        {
            return BadRequest("Invalid JSON payload.");
        }

        if (string.IsNullOrWhiteSpace(scenarioCode))
        {
            return BadRequest("Invalid scenario code.");
        }

        if (await scenarioModelsRepository.GetByIdAsync(scenarioCode, scenarioCode) != null)
        {
            return BadRequest("Requested scenario name is not available.");
        }

        string? logMessage;

        try
        {
            _ = messageParser.ToViewDataPayload(assetPayload);
        }
        catch (AggregateException error)
        {
            var builder = new StringBuilder("View data asset is invalid");
            builder.AppendLine();
            foreach (var ex in error.InnerExceptions)
            {
                builder.AppendLine(ex.Message);
            }

            logMessage = builder.ToString();
            logger.LogError(error, "{message}", logMessage);
            return BadRequest(logMessage);
        }

        var assetId = Guid.NewGuid().ToString();
        var currentMaxCode = await peiModelRepository.GetMaxPartitionKeyAsync();
        var startCode = $"{currentMaxCode + 1:D4}";
        var pei = $"{HolderNameConfigurationId}:{assetId}";

        await AddViewDataModel(assetPayload, assetId);
        await AddPeisDataModel(startCode, assetPayload, pei);
        await AddScenarioModel(scenarioCode, startCode);

        var transformPayload = TransformViewData(assetId, assetPayload, pei);
        var arrangement = GetArrangement(transformPayload);
        var policyString = CreatePolicyContent([arrangement], scenarioCode);

        return Ok(policyString);
    }

    [HttpPost]
    [Route("scenarios/validate")]
    public IActionResult ValidateAsync([FromBody] JsonElement payload)
    {
        var assetPayload = payload.GetRawText();

        if (string.IsNullOrWhiteSpace(assetPayload))
        {
            return BadRequest("Invalid JSON payload.");
        }

        string? logMessage;

        try
        {
            _ = messageParser.ToViewDataPayload(assetPayload);
        }
        catch (AggregateException error)
        {
            var builder = new StringBuilder("View data JSON is invalid");
            builder.AppendLine();
            foreach (var ex in error.InnerExceptions)
            {
                builder.AppendLine(ex.Message);
            }

            logMessage = builder.ToString();
            logger.LogError(error, "{message}", logMessage);
            return BadRequest(logMessage);
        }

        return Ok();
    }

    private async Task AddPeisDataModel(string startCode, string assetPayload, string pei)
    {
        var schemeName = GetPensionScheme(assetPayload);
        var peisModel = new CdaPeisEmulatorScenarioModel
        {
            Id = startCode,
            PeisIdStartCode = startCode,
            DataPoints =
            [
                new() {
                    AvailableAt = 0,
                    ResponsePayload = new ResponsePayload
                    {
                        PeiList =
                        [
                            new() {
                                Description = schemeName,
                                Pei = pei
                            }
                        ]
                    }
                }
            ]
        };

        await peiModelRepository.InsertItemAsync(peisModel, startCode);
    }

    private string TransformViewData(string assetId, string assetPayload, string pei)
    {
        var retrievalId = Guid.NewGuid().ToString();
        return dataTransformer.Transform(assetId, assetPayload, pei, retrievalId);
    }

    private async Task AddViewDataModel(string assetPayload, string assetId)
    {
        var viewDataPayload = new ViewDataPayloadModel
        {
            Id = assetId,
            AssetGuid = assetId,
            ViewData = JObject.Parse(assetPayload)
        };

        await viewDataRepository.InsertItemAsync(viewDataPayload, assetId);
    }

    private async Task AddScenarioModel(string scenarioCode, string startCode)
    {
        var scenarioModel = new TokenEmulatorPiesIdScenarioModel
        {
            Id = scenarioCode,
            Code = scenarioCode,
            PeisIdStartCode = startCode
        };

        await scenarioModelsRepository.InsertItemAsync(scenarioModel, scenarioCode);
    }

    private static string? GetPensionScheme(string viewdataPayload)
    {
        var docRoot = JsonDocument.Parse(viewdataPayload).RootElement;

        if (!docRoot.TryGetProperty("arrangements", out var arrangementsElement) ||
                arrangementsElement.ValueKind != JsonValueKind.Array ||
                arrangementsElement.ValueKind == JsonValueKind.Null ||
                arrangementsElement.GetArrayLength() == 0)
        {
            throw new JsonException("The payload either lacks the 'arrangements' property, or the property is not a valid array.");
        }

        var mainArrangement = arrangementsElement[0];
        return mainArrangement.GetProperty("pensionProviderSchemeName").GetString();
    }

    private static JsonElement GetArrangement(string retrievedPension)
    {
        var docRoot = JsonDocument.Parse(retrievedPension).RootElement;

        if (!docRoot.TryGetProperty("retrievalResult", out var arrangementsElement) ||
                arrangementsElement.ValueKind != JsonValueKind.Array ||
                arrangementsElement.ValueKind == JsonValueKind.Null ||
                arrangementsElement.GetArrayLength() == 0)
        {
            throw new JsonException("The payload either lacks the 'retrievalResult' property, or the property is not a valid array.");
        }

        return arrangementsElement[0];
    }

    private string CreatePolicyContent(IEnumerable<JsonElement> arrangements, string scenarioCode)
    {
        var policy = new { pensionArrangements = arrangements };
        var pension = new { pensionPolicies = new[] { policy }, pensionsDataRetrievalComplete = true, 
            predictedTotalDataRetrievalTime = 80, predictedRemainingDataRetrievalTime = 0};
        var data = new { availableAt = 0, pensionsData = pension };
        var scenario = new { testScenarioCode = scenarioCode, dataPoints = new[] { data } };

        return JsonSerializer.Serialize(scenario, serializerOptions);
    }
}
