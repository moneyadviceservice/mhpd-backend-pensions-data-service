using CDAServiceEmulator.Configuration;
using CDAServiceEmulator.Controllers;
using CDAServiceEmulator.CosmosRepository;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Utils;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Moq;

namespace CDAServiceEmulatorUnitTests;

public class ScenarioControllerTests
{
    private readonly ScenarioController _controller;
    private readonly Mock<IIdValidator> _validator = new();
    private readonly Mock<Container> _scenarioModelContainer = new();
    private readonly Mock<Container> _peisModelContainer = new();
    private readonly Mock<Container> _viewModelContainer = new();
    private readonly Mock<IMessageParser> _messageParser = new();
    private readonly Mock<IViewDataTransformer> _transformer = new();

    public ScenarioControllerTests()
    {
        Mock<ILogger<ScenarioController>> logger = new();

        var configuration = new MhpdCosmosConfiguration
        {
            DatabaseName = "TestDatabase",
            TokenEmulatorPiesIdScenarioModelsContainerName = "tokenEmulatorPiesIdScenarioModels",
            CdaPeisEmulatorScenarioModelContainerName = "cdaPeisEmulatorScenarioModels",
            ViewDataModelContainerName = "viewdatapayloads"
        };

        Mock<CosmosClient> mockCosmosClient = new();
        Mock<Database> database = new();

        mockCosmosClient.Setup(mock => mock.GetDatabase(configuration.DatabaseName))
            .Returns(database.Object);

        database.Setup(mock => mock.GetContainer(configuration.TokenEmulatorPiesIdScenarioModelsContainerName))
            .Returns(_scenarioModelContainer.Object);

        database.Setup(mock => mock.GetContainer(configuration.CdaPeisEmulatorScenarioModelContainerName))
            .Returns(_peisModelContainer.Object);

        database.Setup(mock => mock.GetContainer(configuration.ViewDataModelContainerName))
            .Returns(_viewModelContainer.Object);

        var feedResponse = new Mock<FeedResponse<dynamic>>();
        dynamic responseObject = new { MaxStartCode = 10 };
        feedResponse.Setup(x => x.FirstOrDefault())
            .Returns(responseObject);

        var feedIterator = new Mock<FeedIterator<dynamic>>();
        feedIterator.SetupSequence(x => x.HasMoreResults)
            .Returns(true)
            .Returns(false);
        feedIterator.Setup(x => x.ReadNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(feedResponse.Object);

        _peisModelContainer.Setup(x => x.GetItemQueryIterator<dynamic>(
                It.IsAny<QueryDefinition>(),
                It.IsAny<string>(),
                It.IsAny<QueryRequestOptions>()))
            .Returns(feedIterator.Object);

        var peisModelRepository = new Mock<CdaPeisEmulatorScenarioModelRepository>(
            mockCosmosClient.Object,
            configuration.DatabaseName,
            configuration.CdaPeisEmulatorScenarioModelContainerName
        );

        var scenarioModelRepository = new Mock<TokenEmulatorPiesIdScenarioModelsRepository>(
            mockCosmosClient.Object,
            configuration.DatabaseName,
            configuration.TokenEmulatorPiesIdScenarioModelsContainerName
        );

        var viewdataModelRepository = new Mock<ViewDataRepository>(
            mockCosmosClient.Object,
            configuration.DatabaseName,
            configuration.ViewDataModelContainerName
        );

        var holderNameId = Guid.NewGuid().ToString();
        var assetId = Guid.NewGuid().ToString();
        _validator.Setup(mock => mock.TryExtractPei(It.IsAny<string>(), out holderNameId, out assetId)).Returns(true);

        _messageParser.Setup(mock => mock.ToViewDataPayload(It.IsAny<string>())).Returns(new ViewDataPayload());

        _transformer.Setup(mock => mock.Transform(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(It.IsAny<string>());

        _controller = new ScenarioController(logger.Object, scenarioModelRepository.Object, peisModelRepository.Object, 
            viewdataModelRepository.Object, _transformer.Object, _messageParser.Object, _validator.Object);
    }

    private static string GetValidViewDataPayload()
    {
        return "{\"arrangements\":[{\"pensionProviderSchemeName\":\"Your Pension DC Master Trust\",\"possibleMatchReference\":\"D1006548723\",\"pensionType\":\"DC\",\"pensionOrigin\":\"WM\",\"pensionStatus\":\"A\",\"pensionStartDate\":\"1998-05-16\",\"retirementDate\":\"2038-09-18\",\"dateOfBirth\":\"1973-09-18\",\"possibleMatch\":false,\"pensionAdministrator\":{\"name\":\"Your Pension\",\"contactMethods\":[{\"preferred\":false,\"contactMethodDetails\":{\"email\":\"mastertrust@yourpension.com\"}},{\"preferred\":true,\"contactMethodDetails\":{\"url\":\"https://www.yourpension.co.uk\"}},{\"preferred\":false,\"contactMethodDetails\":{\"number\":\"+44 80080087355\",\"usage\":[\"M\"]}},{\"preferred\":false,\"contactMethodDetails\":{\"postalName\":\"Your Pension\",\"line1\":\"92 Victoria Lane\",\"line2\":\"Frampton Cotterell\",\"line3\":\"Bristol\",\"line4\":\"South Glocustershire\",\"postcode\":\"BS36 9DD\",\"countryCode\":\"GB\"}}]},\"employmentMembershipPeriods\":[{\"employerName\":\"Sweets R Us\",\"employerStatus\":\"C\",\"membershipStartDate\":\"1998-05-16\"}],\"benefitIllustrations\":[{\"illustrationComponents\":[{\"benefitType\":\"DC\",\"calculationMethod\":\"SMPI\",\"payableDetails\":{\"payableDate\":\"2038-09-18\",\"increasing\":false,\"monthlyAmount\":1725,\"annualAmount\":20700,\"amountType\":\"INC\"},\"estimatedDcPot\":300000,\"survivorBenefit\":false,\"safeguardedBenefit\":false},{\"amountType\":\"INC\",\"benefitType\":\"DC\",\"calculationMethod\":\"SMPI\",\"payableDetails\":{\"payableDate\":\"2038-09-18\",\"increasing\":false,\"monthlyAmount\":1351,\"annualAmount\":16215,\"amountType\":\"INC\"},\"accruedDcPot\":235000,\"survivorBenefit\":false,\"safeguardedBenefit\":false}],\"illustrationDate\":\"2023-05-16\"}]}]}";
    }

    private static string GetInvalidViewDataPayload()
    {
        return "{\"arrangements\":[{\"pensionProviderSchemeName\":\"ABC\",\"possibleMatchReference\":\"D9999\",\"pensionType\":\"SP\",\"pensionOrigin\":\"PC\",\"pensionStatus\":\"PC\",\"pensionStartDate\":\"2024-05-05\",\"retirementDate\":\"2042-05-05\",\"dateOfBirth\":\"2000-05-05\",\"possibleMatch\":true,\"pensionAdministrator\":{\"name\":\"ABC Your Pension\",\"contactMethods\":[{\"preferred\":false,\"contactMethodDetails\":{\"email\":\"abcmastertrust@yourpension.com\"}},{\"preferred\":true,\"contactMethodDetails\":{\"url\":\"https://www.abcyourpension.co.uk\"}},{\"preferred\":false,\"contactMethodDetails\":{\"number\":\"+44 9999999999\",\"usage\":[\"A\"]}},{\"preferred\":false,\"contactMethodDetails\":{\"postalName\":\"ABCYour Pension\",\"line1\":\"92 Victoria Lane\",\"line2\":\"Frampton Cotterell\",\"line3\":\"Bristol\",\"line4\":\"South Glocustershire\",\"postcode\":\"BS36 9DD\",\"countryCode\":\"GB\"}}]},\"employmentMembershipPeriods\":[{\"employerName\":\"ABCSweets R Us\",\"employerStatus\":\"H\",\"employmentStartDate\":\"1998-05-16\"}],\"benefitIllustrations\":[{\"illustrationComponents\":[{\"benefitType\":\"MHPD\",\"calculationMethod\":\"SMPI\",\"payableDetails\":{\"payableDate\":\"2038-09-18\",\"annualAmount\":20700,\"amountType\":\"INC\"},\"estimatedDcPot\":300000,\"survivorBenefit\":false,\"safeguardedBenefit\":false},{\"benefitType\":\"DC\",\"calculationMethod\":\"SMPI\",\"payableDetails\":{\"payableDate\":\"2038-09-18\",\"annualAmount\":16215,\"amountType\":\"INC\"},\"accruedDcPot\":235000,\"survivorBenefit\":false,\"safeguardedBenefit\":false}],\"illustrationDate\":\"2030-05-05\"}]}]}";

    }
}
