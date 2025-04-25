using CDAServiceEmulator.Controllers;
using CDAServiceEmulator.CosmosRepository;
using CDAServiceEmulator.Models.Peis;
using CDAServiceEmulator.Models.Token;
using CDAServiceEmulator.Models.ViewData;
using MhpdCommon.Models.Configuration;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Repository;
using MhpdCommon.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace CDAServiceEmulatorUnitTests;

public class ScenarioControllerTests
{
    private const int MaxScenarioCode = 100;
    private readonly ScenarioController _controller;
    private readonly Mock<IIdValidator> _validator = new();
    private readonly Mock<ICosmosDbRepository<TokenEmulatorPiesIdScenarioModel>> _scenarioModelRepository = new();
    private readonly Mock<ICdaPeisEmulatorScenarioModelRepository> _peisModelRepository = new();
    private readonly Mock<ICosmosDbRepository<ViewDataPayloadModel>> _viewModelRepository = new();
    private readonly Mock<IMessageParser> _messageParser = new();
    private readonly Mock<IViewDataTransformer> _transformer = new();

    public ScenarioControllerTests()
    {
        Mock<ILogger<ScenarioController>> logger = new();

        var configuration = new CosmosTestHarnessConfiguration
        {
            DatabaseName = "TestDatabase",
            TokenEmulatorPiesIdScenarioModelsContainerName = "tokenEmulatorPiesIdScenarioModels",
            CdaPeisEmulatorScenarioModelContainerName = "cdaPeisEmulatorScenarioModels",
            ViewDataModelContainerName = "viewdatapayloads"
        };

        _scenarioModelRepository.Setup(mock => mock.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(TokenEmulatorPiesIdScenarioModel());

        _scenarioModelRepository.Setup(mock => mock.InsertItemAsync(It.IsAny<TokenEmulatorPiesIdScenarioModel>(), It.IsAny<string>()))
            .Verifiable();

        _scenarioModelRepository.Setup(mock => mock.DeleteByIdAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        _scenarioModelRepository.Setup(mock => mock.GetAllAsync())
            .ReturnsAsync([new TokenEmulatorPiesIdScenarioModel { IsHiddenScenario = true }, new()]);

        _peisModelRepository.Setup(mock => mock.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(CdaPeisEmulatorScenarioModel());

        _peisModelRepository.Setup(mock => mock.InsertItemAsync(It.IsAny<CdaPeisEmulatorScenarioModel>(), It.IsAny<string>()))
            .Verifiable();

        _peisModelRepository.Setup(mock => mock.DeleteByIdAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        _peisModelRepository.Setup(mock => mock.GetMaxScenarioCodeAsync())
            .ReturnsAsync(MaxScenarioCode);

        _viewModelRepository.Setup(mock => mock.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(ViewDataPayloadModel());

        _viewModelRepository.Setup(mock => mock.InsertItemAsync(It.IsAny<ViewDataPayloadModel>(), It.IsAny<string>()))
            .Verifiable();

        _viewModelRepository.Setup(mock => mock.DeleteByIdAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        var holderNameId = Guid.NewGuid().ToString();
        var assetId = Guid.NewGuid().ToString();
        _validator.Setup(mock => mock.TryExtractPei(It.IsAny<string>(), out holderNameId, out assetId)).Returns(true);

        _messageParser.Setup(mock => mock.ToViewDataPayload(It.IsAny<string>())).Returns(new ViewDataPayload());

        _transformer.Setup(mock => mock.Transform(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(GetTransformedPension());

        _controller = new ScenarioController(logger.Object, _scenarioModelRepository.Object, _peisModelRepository.Object,
            _viewModelRepository.Object, _transformer.Object, _messageParser.Object, _validator.Object);
    }

    [Fact]
    public async Task GetScenarioById_ValidScenario_ReturnsOk()
    {
        // Act
        var result = await _controller.GetAsync("scenarioCode");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult);
    }

    [Fact]
    public async Task GetScenarioById_MissingScenario_ReturnsNotFound()
    {
        // Arrange
        _scenarioModelRepository.Setup(mock => mock.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((TokenEmulatorPiesIdScenarioModel?)null);

        // Act
        var result = await _controller.GetAsync("invalidScenarioCode");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetScenarioById_MissingPeis_ReturnsNotFound()
    {
        // Arrange
        _peisModelRepository.Setup(mock => mock.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((CdaPeisEmulatorScenarioModel?)null);

        // Act
        var result = await _controller.GetAsync("invalidScenarioCode");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetAllScenarios_ReturnsSingle()
    {
        // Act
        var result = await _controller.GetAllAsync();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult);
        Assert.NotNull(okResult.Value);
        Assert.Single((List<TokenEmulatorPiesIdScenarioModel>)okResult.Value);
    }

    [Fact]
    public async Task GetAllScenarios_ReturnsMultiple()
    {
        // Arrange
        _scenarioModelRepository.Setup(mock => mock.GetAllAsync())
            .ReturnsAsync([new(), new()]);

        // Act
        var result = await _controller.GetAllAsync();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult);
        Assert.NotNull(okResult.Value);
        Assert.Equal(2, ((List<TokenEmulatorPiesIdScenarioModel>)okResult.Value).Count);
    }

    [Fact]
    public async Task PostScenario_InvalidScenario_ReturnsBadRequest()
    {
        // Arrange
        var payload = JsonDocument.Parse(GetSingleArrangementsPayload()).RootElement;

        // Act
        var result = await _controller.PostAsync(payload, string.Empty);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task PostScenario_ExistingScenario_ReturnsBadRequest()
    {
        // Arrange
        var payload = JsonDocument.Parse(GetSingleArrangementsPayload()).RootElement;

        // Act
        var result = await _controller.PostAsync(payload, "ScenarioCode");

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task PostScenario_InvalidPayload_ReturnsBadRequest()
    {
        // Arrange
        _scenarioModelRepository.Setup(mock => mock.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((TokenEmulatorPiesIdScenarioModel?)null);

        var error = new InvalidOperationException("Invalid payload");
        _messageParser.Setup(mock => mock.ToViewDataPayload(It.IsAny<string>())).Throws(new AggregateException(error));

        var payload = JsonDocument.Parse(GetSingleArrangementsPayload()).RootElement;

        // Act
        var result = await _controller.PostAsync(payload, "ScenarioCode");

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PostScenario_ValidScenario_ReturnsOk(bool useMultipleArrangementPayload)
    {
        // Arrange
        _scenarioModelRepository.Setup(mock => mock.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((TokenEmulatorPiesIdScenarioModel?)null);

        var assetCount = useMultipleArrangementPayload ? 3 : 1;
        var asset = useMultipleArrangementPayload ? GetMultileArrangementsPayload() : GetSingleArrangementsPayload();
        var payload = JsonDocument.Parse(asset).RootElement;

        var scenarioCode = "ScenarioCode";

        var startCode = $"{MaxScenarioCode + 1:D4}";

        // Act
        var result = await _controller.PostAsync(payload, scenarioCode);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult);
        _scenarioModelRepository.Verify(mock => mock.InsertItemAsync(It.IsAny<TokenEmulatorPiesIdScenarioModel>(), scenarioCode), Times.Once);
        _peisModelRepository.Verify(mock => mock.InsertItemAsync(It.IsAny<CdaPeisEmulatorScenarioModel>(), startCode), Times.Once);
        _viewModelRepository.Verify(mock => mock.InsertItemAsync(It.IsAny<ViewDataPayloadModel>(), It.IsAny<string>()), Times.Exactly(assetCount));
    }

    [Fact]
    public void ValidateScenario_InvalidPayload_ReturnsBadRequest()
    {
        // Arrange
        var error = new InvalidOperationException("Invalid payload");
        _messageParser.Setup(mock => mock.ToViewDataPayload(It.IsAny<string>())).Throws(new AggregateException(error));

        var payload = JsonDocument.Parse(GetSingleArrangementsPayload()).RootElement;

        // Act
        var result = _controller.Validate(payload);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public void ValidateScenario_ValidPayload_ReturnsBadRequest()
    {
        // Arrange
        var payload = JsonDocument.Parse(GetSingleArrangementsPayload()).RootElement;

        // Act
        var result = _controller.Validate(payload);

        // Assert
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task DeleteScenario_ReturnsOk()
    {
        // Arrange
        var scenarios = new List<string> { "First", "Second", "Third", "Fourth"};

        // Act
        var result = await _controller.DeleteAsync(scenarios);

        // Assert
        _scenarioModelRepository.Verify(mock => mock.DeleteByIdAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(scenarios.Count));
        _peisModelRepository.Verify(mock => mock.DeleteByIdAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(scenarios.Count));
        _viewModelRepository.Verify(mock => mock.DeleteByIdAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(scenarios.Count));
    }

    private static string GetSingleArrangementsPayload()
    {
        return "{\"arrangements\":[{\"possibleMatch\":false,\"pensionProviderSchemeName\":\"State Pension\",\"pensionType\":\"SP\"}]}";
    }

    private static string GetMultileArrangementsPayload()
    {
        return "{\"arrangements\":[{\"possibleMatch\":false,\"pensionProviderSchemeName\":\"State Pension\",\"pensionType\":\"SP\"},{\"possibleMatch\":false,\"pensionProviderSchemeName\":\"DC Scheme\",\"pensionType\":\"DC\"},{\"possibleMatch\":false,\"pensionProviderSchemeName\":\"DB Scheme\",\"pensionType\":\"DB\"}]}";

    }

    private static string GetTransformedPension()
    {
        return "{\"retrievalResult\":[{\"externalAssetId\":\"14343\",\"matchType\":\"DEFN\",\"schemeName\":\"MyCompany Direct Contribution Scheme\"}]}";
    }

    private static TokenEmulatorPiesIdScenarioModel TokenEmulatorPiesIdScenarioModel()
    {
        return new TokenEmulatorPiesIdScenarioModel
        {
            Code = "ScenarioCode",
            PeisIdStartCode = "PeisScenarioCode",
            IsHiddenScenario = false
        };
    }

    private static CdaPeisEmulatorScenarioModel CdaPeisEmulatorScenarioModel()
    {
        return new CdaPeisEmulatorScenarioModel
        {
            Id = "PeisScenarioCode",
            PeisIdStartCode = "ScenarioCode",
            DataPoints =
            [
                new() {
                    AvailableAt = 0,
                    ResponsePayload = new ResponsePayload
                    {
                        PeiList =
                        [
                            new() {
                                Pei = "Pei",
                                Description = "Description",
                            }
                        ],
                    }
                }
            ],
        };
    }

    private static ViewDataPayloadModel ViewDataPayloadModel()
    {
        return new ViewDataPayloadModel
        {
            Id = "PeisScenarioCode",
            AssetGuid = "ScenarioCode",
            ViewData = JObject.Parse(GetSingleArrangementsPayload()),
        };
    }
}