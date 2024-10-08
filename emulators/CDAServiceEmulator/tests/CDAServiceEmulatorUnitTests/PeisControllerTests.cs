using System.Net;
using CDAServiceEmulator.Configuration;
using CDAServiceEmulator.Controllers;
using CDAServiceEmulator.CosmosRepository;
using CDAServiceEmulator.Models.Peis;
using CDAServiceEmulatorUnitTests.Mock.ScenarioModelData;
using MhpdCommon.Models.RequestHeaderModel;
using MhpdCommon.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Moq;

namespace CDAServiceEmulatorUnitTests;

public class PeisControllerTests
{
    private readonly DefaultHttpContext _httpContext;
    private readonly PeisController _controller;
    private readonly Mock<IOptions<MhpdCosmosConfiguration>> mockCosmosConfigOptions;
    private readonly Mock<Container> mockScenarioModelContainer;
    private readonly Mock<Container> mockTestInstanceContainer;
    private readonly Mock<IIdValidator> mockIdValidatorMock;
    
    public PeisControllerTests()
    {
        var configuration = new MhpdCosmosConfiguration
        {
            DatabaseName = "TestDatabase",
            CdaPeisEmulatorScenarioModelContainerName = "ScenarioModelContainer",
            CdaPeisEmulatorTestInstanceDataContainerName = "TestInstanceContainer",
        };

        Mock<CosmosClient> mockCosmosClient = new();
        mockScenarioModelContainer = new();
        mockTestInstanceContainer = new();
        Mock<Database> mockDatabase = new();

        mockCosmosClient.Setup(mock => mock.GetDatabase(configuration.DatabaseName))
            .Returns(mockDatabase.Object);

        mockDatabase.Setup(mock => mock.GetContainer(configuration.CdaPeisEmulatorScenarioModelContainerName))
            .Returns(mockScenarioModelContainer.Object);
        
        mockDatabase.Setup(mock => mock.GetContainer(configuration.CdaPeisEmulatorTestInstanceDataContainerName))
            .Returns(mockTestInstanceContainer.Object);
        
        mockIdValidatorMock = new Mock<IIdValidator>();
        mockIdValidatorMock.Setup(x => x.IsValidGuid(It.IsAny<string>())).Returns(true);
        mockIdValidatorMock.Setup(x => x.IsValidPeI(It.IsAny<string>())).Returns(false);
        
        mockCosmosConfigOptions = new Mock<IOptions<MhpdCosmosConfiguration>>();
        mockCosmosConfigOptions.Setup(x => x.Value).Returns(configuration);
        
        // Instantiate the CdaPeisEmulatorScenarioModelRepository with the mocked CosmosClient and configuration
        var mockScenarioModelRepository = new Mock<CdaPeisEmulatorScenarioModelRepository>(
            mockCosmosClient.Object, 
            configuration.DatabaseName, 
            configuration.CdaPeisEmulatorScenarioModelContainerName
        );
        
        var mockTestInstanceRepository = new Mock<CdaPeisEmulatorTestInstanceDataRepository>(
            mockCosmosClient.Object, 
            configuration.DatabaseName, 
            configuration.CdaPeisEmulatorTestInstanceDataContainerName
        );

        _httpContext = new DefaultHttpContext();

        // Inject mocks into the controller
        _controller = new PeisController(mockScenarioModelRepository.Object, mockTestInstanceRepository.Object, mockIdValidatorMock.Object)
        {
            ControllerContext = new ControllerContext()
            {
                HttpContext = _httpContext
            }
        };
    }

    [Fact]
    public async void WhenControllerIsCalled_WithCorrectAuthorisationHeader_InCorrectPiesRoute_ThenItShouldReturn_BadRequest400Response()
    {
        // Arrange
        AddAuthorisationHeader();
        var xRequestId = "b7301d11-f166-499a-9bf1-0598c2f1af52";
        string peis_id = "?><>(*)&&-8586-4483-9899-17dd85af9074";
        
        mockIdValidatorMock.Setup(x => x.IsValidGuid(It.IsAny<string>())).Returns(true);
        mockIdValidatorMock.Setup(x => x.IsValidGuid(peis_id)).Returns(false);
        
        // Act
        var result = await _controller.GetAsync("", new RequestHeaderModel { XRequestId = xRequestId });
        BadRequestObjectResult badResult = (BadRequestObjectResult)result;

        // Assert
        Assert.True(result.GetType() == typeof(BadRequestObjectResult));
        Assert.True(badResult.StatusCode == (int)HttpStatusCode.BadRequest);
        Assert.True((string)badResult.Value! == "Invalid peis_id");

    }

    [Fact]
    public async void WhenControllerIsCalled_WithInCorrectAuthorisationHeader_CorrectRoutePies_ThenItShouldReturn_UnAuthorised401Response()
    {
        // Arrange
        AddInCorrectAuthorisationHeader();
        var xRequestId = "b7301d11-f166-499a-9bf1-0598c2f1af52";
        string peis_id = "8586-4483-9899-17dd85af9074";

        // Act
        var result = await _controller.GetAsync(peis_id, new RequestHeaderModel { XRequestId = xRequestId });

        // Assert
        Assert.True(result.GetType() == typeof(UnauthorizedObjectResult));
    }

    [Fact]
    public async void WhenControllerIsCalled_WithCorrectHeaders_CorrectPath_No_X_Request_Id_ThenItShouldReturn_BadRequest400Response()
    {
        // Arrange
        AddAuthorisationHeader();
        string peis_id = "8586-4483-9899-17dd85af9074";

        // Act
        var result = await _controller.GetAsync(peis_id, new RequestHeaderModel { XRequestId = null });
        BadRequestObjectResult badResult = (BadRequestObjectResult)result;

        // Assert
        Assert.True(result.GetType() == typeof(BadRequestObjectResult));
        Assert.True(badResult.StatusCode == (int)HttpStatusCode.BadRequest);
        Assert.True((string)badResult.Value! == "Invalid X-Request-Id");
    }

    [Fact]
    public async void WhenControllerIsCalled_WithNoAuthorisationHeader_CorrectRoutePies_ThenItShouldReturn_Unauthorised401ResponseAnd_WwwAuthenticateResponseHeader()
    {
        // Arrange
        string peis_id = "8586-4483-9899-17dd85af9074";
        var xRequestId = "b7301d11-f166-499a-9bf1-0598c2f1af52";
        var responseHeaderValue = "realm=\"PensionDashboard\", as_uri=\"https://as.pdp.com\", ticket=\"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.cThIIoDvwdueQB468K5xDc5633seEFoqwxjF_xSJyQQ\"";

        // Act
        var result = await _controller.GetAsync(peis_id, new RequestHeaderModel { XRequestId = xRequestId });
        UnauthorizedObjectResult unAuthorizedResult = (UnauthorizedObjectResult)result;
        _httpContext.Response.Headers.TryGetValue("WWW-Authenticate", out var wwwAuthenticate);

        // Assert
        Assert.True(result.GetType() == typeof(UnauthorizedObjectResult));
        Assert.True(unAuthorizedResult.StatusCode == (int)HttpStatusCode.Unauthorized);
        Assert.True(wwwAuthenticate == responseHeaderValue);
        Assert.True((string)unAuthorizedResult.Value! == "Unauthorized");
    }
        
    [Fact]
    public async void WhenControllerIsCalled_WithCorrectAuthorisationHeader_CorrectRoute_No_Model_ThenItShouldReturn_BadRequest()
    {
        // Arrange
        AddAuthorisationHeader();
        var xRequestId = "b7301d11-f166-499a-9bf1-0598c2f1af52";
        string peis_id = "cd0e4fdc-8586-4483-9899-17dd85af9074";
        
        // Arrange
        mockScenarioModelContainer
            .Setup(c => c.ReadItemAsync<CdaPeisEmulatorScenarioModel>(It.IsAny<string>(), It.IsAny<PartitionKey>(), null, default))
            .ThrowsAsync(new CosmosException("Not Found", HttpStatusCode.NotFound, 0, "", 0)); // Mock a not found exception

        // Act
        var result = await _controller.GetAsync(peis_id, new RequestHeaderModel { XRequestId = xRequestId });
        BadRequestObjectResult badResult = (BadRequestObjectResult)result;

        // Assert
        Assert.True(result.GetType() == typeof(BadRequestObjectResult));
        Assert.True(badResult.StatusCode == (int)HttpStatusCode.BadRequest);
        Assert.True((string)badResult.Value! == "Unknown test scenario");
    }

    [Fact]
    public async Task WhenControllerIsCalled_WithCorrectAuthorisationHeader_CorrectRoute_Peis_Matched_ThenItShouldReturn_ResponsePayLoad()
    {
        // Arrange
        AddAuthorisationHeader();
        var xRequestId = "b7301d11-f166-499a-9bf1-0598c2f1af52";
        string peis_id = "0001fdcd-8586-4483-9899-17dd85af9074";

        // Load the JSON data as payload
        var data = DataProvider.GetPayload<CdaPeisEmulatorScenarioModel>("0002.json");

        // Extract the responsePayload from the first dataPoint
        var responsePayload = data?.DataPoints?[0].ResponsePayload;

        // Mock the ItemResponse<CdaPeisEmulatorScenarioModel>
        var mockItemResponse = new Mock<ItemResponse<CdaPeisEmulatorScenarioModel>>();
        if (data != null)
        {
            mockItemResponse.Setup(x => x.Resource).Returns(data);

            // Set up the mockContainer to return the mocked ItemResponse
            mockScenarioModelContainer
                .Setup(c => c.ReadItemAsync<CdaPeisEmulatorScenarioModel>(It.IsAny<string>(), It.IsAny<PartitionKey>(),
                    null, default))
                .ReturnsAsync(mockItemResponse.Object); // Return the mock ItemResponse object

            // Arrange
            mockTestInstanceContainer
                .Setup(c => c.ReadItemAsync<CdaPeisEmulatorTestInstanceDataModel>(It.IsAny<string>(),
                    It.IsAny<PartitionKey>(), null, default))
                .ThrowsAsync(new CosmosException("Not Found", HttpStatusCode.NotFound, 0, "",
                    0)); // Mock a not found exception

            // Act
            var result = await _controller.GetAsync(peis_id, new RequestHeaderModel { XRequestId = xRequestId });

            // Assert
            Assert.IsType<OkObjectResult>(result); // Check if the result is an OkObjectResult

            var okResult = result as OkObjectResult;

            // Verify the responsePayload is not null
            Assert.NotNull(responsePayload); // Ensure the response payload is not null
            Assert.True(responsePayload.PeiList?.Count > 0,
                "The response payload should contain at least one PEI.");

            // Verify the result data matches the expected payload
            var returnedModel = okResult?.Value as ResponsePayload;
            Assert.NotNull(returnedModel);
            Assert.Equal(data.DataPoints?[0].ResponsePayload?.PeiList?.Count, returnedModel.PeiList?.Count);
        }
    }

    [Theory]
    [InlineData("0002", 0, 0)]
    [InlineData("0003", 1, -5)]
    [InlineData("0004", 1, -10)]
    [InlineData("0004", 2, -19)]
    [InlineData("0009", 0, 0)]
    public async Task WhenControllerIsCalled_WithCorrectAuthorisationHeader_CorrectRoute_Peis_Matched_All_Scenarios_ThenItShouldReturn_ResponsePayLoad(string scenarioName, int expectedDataPoint, int pastSeconds)
    {
        // Arrange
        AddAuthorisationHeader();
       var xRequestId = "b7301d11-f166-499a-9bf1-0598c2f1af52";
        string peis_id = scenarioName + "fdcd-8586-4483-9899-17dd85af9074";

        // Load the JSON data as payload
        var data = DataProvider.GetPayload<CdaPeisEmulatorScenarioModel>(scenarioName +".json");

        // Extract the responsePayload from the first dataPoint
        var responsePayload = data?.DataPoints?[expectedDataPoint].ResponsePayload;

        // Mock the ItemResponse<CdaPeisEmulatorScenarioModel>
        var mockItemResponse = new Mock<ItemResponse<CdaPeisEmulatorScenarioModel>>();
        if (data != null)
        {
            mockItemResponse.Setup(x => x.Resource).Returns(data);

            // Set up the mockContainer to return the mocked ItemResponse
            mockScenarioModelContainer
                .Setup(c => c.ReadItemAsync<CdaPeisEmulatorScenarioModel>(It.IsAny<string>(), It.IsAny<PartitionKey>(),
                    null, default))
                .ReturnsAsync(mockItemResponse.Object); // Return the mock ItemResponse object

            var testInstanceData = new CdaPeisEmulatorTestInstanceDataModel
            {
                Id = scenarioName,
                PeisId = scenarioName,
                InitialCallTimestamp =
                    DateTimeOffset.UtcNow.AddSeconds(pastSeconds) // Mock the request to be in the past
            };

            // Mock the ItemResponse<CdaPeisEmulatorScenarioModel>
            var mockItemTestInstanceResponse = new Mock<ItemResponse<CdaPeisEmulatorTestInstanceDataModel>>();
            mockItemTestInstanceResponse.Setup(x => x.Resource).Returns(testInstanceData);

            mockTestInstanceContainer
                .Setup(c => c.ReadItemAsync<CdaPeisEmulatorTestInstanceDataModel>(It.IsAny<string>(),
                    It.IsAny<PartitionKey>(), null, default))
                .ReturnsAsync(mockItemTestInstanceResponse.Object);

            // Act
            var result = await _controller.GetAsync(peis_id, new RequestHeaderModel { XRequestId = xRequestId });

            // Assert
            Assert.IsType<OkObjectResult>(result); // Check if the result is an OkObjectResult

            var okResult = result as OkObjectResult;

            // Verify the responsePayload is not null
            Assert.NotNull(responsePayload); // Ensure the response payload is not null

            if (scenarioName != "0009")
            {
                Assert.True(responsePayload.PeiList?.Count > 0,
                    "The response payload should contain at least one PEI.");
            }

            // Verify the result data matches the expected payload
            var returnedModel = okResult?.Value as ResponsePayload;
            Assert.NotNull(returnedModel);
            Assert.Equal(data.DataPoints?[expectedDataPoint].ResponsePayload?.PeiList?.Count,
                returnedModel.PeiList?.Count);
        }
    }
    
    [Theory]
    [MemberData(nameof(GetAvailableAtTestCases))]
    public void FindClosestAvailableTime_ReturnsClosestTime(int[] availableAts, int timeSince, int expected)
    {
        // Act
        var closestTime = PeisController.FindClosestAvailableTime(availableAts.ToList(), timeSince);

        // Assert
        Assert.Equal(expected, closestTime);
    }
    
    [Theory]
    [InlineData("0002", 0, 0)]
    [InlineData("0003", 1, 5)]
    [InlineData("0004", 1, 8)]
    [InlineData("0004", 2, 18)]
    [InlineData("0009", 0, 0)]
    public void FindResponsePayload_ReturnsCorrectPayload(string scenarioName, int dataPoint, int closestAvailableAt)
    {
        // Arrange
        var scenarioModelData = DataProvider.GetPayload<CdaPeisEmulatorScenarioModel>(scenarioName + ".json");

        // Act
        if (scenarioModelData != null)
        {
            var result = PeisController.FindResponsePayload(scenarioModelData, closestAvailableAt);

            // Verify the result data matches the expected payload
            var returnedModel = result;
            Assert.NotNull(returnedModel);
            Assert.Equal(scenarioModelData.DataPoints?[dataPoint].ResponsePayload?.PeiList?.Count, returnedModel.PeiList?.Count);
        }
    }
    
    private void AddAuthorisationHeader()
    {
        _httpContext.Request.Headers[HeaderNames.Authorization] = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
    }

    private void AddInCorrectAuthorisationHeader()
    {
        _httpContext.Request.Headers[HeaderNames.Authorization] = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
    }
    
        
    // Static readonly array
    private static readonly int[] AvailableTimeSet1 = [10, 20, 30, 40];
    private static readonly int[] AvailableTimeSet2 = [0, 5];
    private static readonly int[] AvailableTimeSet3 = [0, 8, 18];
    
    // Method to provide test data
    public static IEnumerable<object[]> GetAvailableAtTestCases()
    {
        return new List<object[]>
        {
            new object[] { AvailableTimeSet1, 25, 20 },
            new object[] { AvailableTimeSet2, 6, 5 },
            new object[] { AvailableTimeSet2, 4, 0 },
            new object[] { AvailableTimeSet3, 7, 0 },
            new object[] { AvailableTimeSet3, 9, 8 },
            new object[] { AvailableTimeSet3, 17, 8 },
            new object[] { AvailableTimeSet3, 25, 18 }
        };
    }
}