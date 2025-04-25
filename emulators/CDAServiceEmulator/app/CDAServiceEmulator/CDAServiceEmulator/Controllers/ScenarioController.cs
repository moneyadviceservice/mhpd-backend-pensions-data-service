using CDAServiceEmulator.CosmosRepository;
using CDAServiceEmulator.Models.Peis;
using CDAServiceEmulator.Models.Token;
using CDAServiceEmulator.Models.ViewData;
using MhpdCommon.Constants;
using MhpdCommon.Repository;
using MhpdCommon.Utils;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Text.Json;

namespace CDAServiceEmulator.Controllers;

[Route("/")]
[ApiController]
public class ScenarioController(ILogger<ScenarioController> logger,
    ICosmosDbRepository<TokenEmulatorPiesIdScenarioModel> scenarioModelsRepository,
    ICdaPeisEmulatorScenarioModelRepository peiModelRepository,
    ICosmosDbRepository<ViewDataPayloadModel> viewDataRepository,
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
        var validScenarios = scenarios.Where(scenario => !scenario.IsHiddenScenario).ToList();

        return Ok(validScenarios);
    }

    [HttpPost]
    [Route("scenarios")]
    public async Task<IActionResult> PostAsync([FromBody] JsonElement payload, [FromHeader] string scenarioCode)
    {
        var assetPayload = payload.GetRawText();

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
        var arrangements = SplitArrangements(payload);

        var currentMaxCode = await peiModelRepository.GetMaxScenarioCodeAsync();
        var startCode = $"{currentMaxCode + 1:D4}";

        var transformedArrangements = new List<JsonElement>();
        var peiModel = CreatePeisDataModel(startCode);

        foreach (var arrangement in arrangements)
        {
            var arrangementPayload = arrangement.GetRawText();
            var assetId = Guid.NewGuid().ToString();
            var pei = $"{HolderNameConfigurationId}:{assetId}";
            await AddViewDataModel(arrangementPayload, assetId);
            AddPeisDataPoint(peiModel, arrangementPayload, pei);
            var retrievedPension = TransformViewData(assetId, arrangementPayload, pei);
            var transformedArrangement = GetArrangement(retrievedPension);
            transformedArrangements.Add(transformedArrangement);
        }

        await SavePeisDataModel(peiModel);
        await AddScenarioModel(scenarioCode, startCode);

        var policies = CreatePolicyContent(transformedArrangements, scenarioCode);

        return Ok(policies);
    }

    [HttpPost]
    [Route("scenarios/validate")]
    public IActionResult Validate([FromBody] JsonElement payload)
    {
        var assetPayload = payload.GetRawText();

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

    [HttpDelete]
    [Route("scenarios")]
    public async Task<IActionResult> DeleteAsync([FromBody] List<string> scenarioCodes)
    {
        ArgumentNullException.ThrowIfNull(scenarioCodes);

        var deleteCounter = 0;

        foreach (var scenarioCode in scenarioCodes)
        {
            if (await DeleteAsync(scenarioCode))
            {
                deleteCounter++;
            }
        }

        return Ok($"Removed data for {deleteCounter} out of {scenarioCodes.Count} scenarios ");
    }

    private async Task<bool> DeleteAsync(string scenarioCode)
    {
        if(scenarioCode == null) return false;

        var scenario = await scenarioModelsRepository.GetByIdAsync(scenarioCode, scenarioCode);

        if (scenario == null) return false;

        var peiModel = await peiModelRepository.GetByIdAsync(scenario.PeisIdStartCode!, scenario.PeisIdStartCode!);

        if (peiModel == null) return false;

        var peis = peiModel.DataPoints!.SelectMany(point => point.ResponsePayload!.PeiList!).DistinctBy(pei => pei.Pei);

        foreach (var pei in peis)
        {
            if (idValidator.TryExtractPei(pei.Pei, out _, out var assetId))
            {
                await viewDataRepository.DeleteByIdAsync(assetId, assetId);
            }
        }

        await peiModelRepository.DeleteByIdAsync(scenario.PeisIdStartCode!, scenario.PeisIdStartCode!);

        await scenarioModelsRepository.DeleteByIdAsync(scenarioCode, scenarioCode);

        return true;
    }

    private static List<JsonElement> SplitArrangements(JsonElement payload)
    {
        if (!payload.TryGetProperty(PensionConstants.Arrangements, out JsonElement arrangements) ||
            arrangements.ValueKind != JsonValueKind.Array)
        {
            throw new ArgumentException($"Missing or invalid '{PensionConstants.Arrangements}' array.");
        }

        var results = new List<JsonElement>();

        foreach (var arrangement in arrangements.EnumerateArray())
        {
            var newPayload = new Dictionary<string, object?>
            {
                ["arrangements"] = new[] { JsonSerializer.Deserialize<object>(arrangement.GetRawText()) }
            };

            string newJson = JsonSerializer.Serialize(newPayload);
            var newElement = JsonDocument.Parse(newJson).RootElement;
            results.Add(newElement);
        }

        return results;
    }

    private static CdaPeisEmulatorScenarioModel CreatePeisDataModel(string startCode)
    {
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
                        PeiList = []
                    }
                }
            ]
        };

        return peisModel;
    }

    private static void AddPeisDataPoint(CdaPeisEmulatorScenarioModel peisModel, string assetPayload, string pei)
    {
        var schemeName = GetPensionScheme(assetPayload);

        peisModel.DataPoints![0].ResponsePayload!.PeiList!.Add(new PeiItem
        {
            Description = schemeName,
            Pei = pei
        });
    }

    private async Task SavePeisDataModel(CdaPeisEmulatorScenarioModel peisModel)
    {
        await peiModelRepository.InsertItemAsync(peisModel, peisModel.PeisIdStartCode!);
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

        return arrangementsElement;
    }

    private string CreatePolicyContent(IEnumerable<JsonElement> arrangements, string scenarioCode)
    {
        var policies = arrangements.Select(arrangement => new { pensionArrangements = arrangement });
        var pension = new { pensionPolicies = policies, pensionsDataRetrievalComplete = true, 
            predictedTotalDataRetrievalTime = 80, predictedRemainingDataRetrievalTime = 0};
        var data = new { availableAt = 0, pensionsData = pension };
        var scenario = new { testScenarioCode = scenarioCode, dataPoints = new[] { data } };

        return JsonSerializer.Serialize(scenario, serializerOptions);
    }
}
