using CDAServiceEmulator.Configuration;
using CDAServiceEmulator.Controllers;
using CDAServiceEmulator.CosmosRepository;
using CDAServiceEmulator.Models;
using CDAServiceEmulator.Models.Token;
using MhpdCommon.Models.Configuration;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Models.RequestHeaderModel;
using MhpdCommon.TokenValidation;
using MhpdCommon.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace CDAServiceEmulatorUnitTests;

public class CdaTokenControllerTests
{
    private readonly Mock<IIdValidator> _mockIdValidator;
    private readonly CdaTokenController _controller;
    private readonly Mock<Container> _mockScenarioModelContainer;

    public CdaTokenControllerTests()
    {
        var configuration = new MhpdCosmosConfiguration
        {
            DatabaseName = "TestDatabase",
            TokenEmulatorPiesIdScenarioModelsContainerName = "TokenEmulatorPiesIdScenarioModelsRepository",
        };
        
        Mock<ILogger<CdaTokenController>> mockLogger = new();
        _mockIdValidator = new Mock<IIdValidator>();
        
        Mock<CosmosClient> mockCosmosClient = new();
        _mockScenarioModelContainer = new();
        Mock<Database> mockDatabase = new();
        
        mockCosmosClient.Setup(mock => mock.GetDatabase(configuration.DatabaseName))
            .Returns(mockDatabase.Object);

        mockDatabase.Setup(mock => mock.GetContainer(configuration.TokenEmulatorPiesIdScenarioModelsContainerName))
            .Returns(_mockScenarioModelContainer.Object);
                
        Mock<IOptions<MhpdCosmosConfiguration>> mockCosmosConfigOptions = new();
        mockCosmosConfigOptions.Setup(x => x.Value).Returns(configuration);
        
        // Instantiate the TokenEmulatorPiesIdScenarioModelsRepository with the mocked CosmosClient and configuration
        var mockScenarioModelRepository = new Mock<TokenEmulatorPiesIdScenarioModelsRepository>(
            mockCosmosClient.Object, 
            configuration.DatabaseName, 
            configuration.TokenEmulatorPiesIdScenarioModelsContainerName
        );

        // Get ordered validators
        var validators = Helper.GetOrderedValidators();

        // Create the TokenRequestValidatorPipeline with the mock validators
        Mock<TokenRequestValidatorPipeline> mockValidatorPipeline = new(validators);

        // Setup Jwt private key mock
        var jwtConfiguration = new JwtSettings
        {
            PrivateKey = Helper.GeneratedRsaPrivateKeyPem
        };

        Mock<IOptions<JwtSettings>> mockJwtSettingsOptions = new();
        mockJwtSettingsOptions.Setup(x => x.Value).Returns(jwtConfiguration);

        var utils = new TokenUtility(mockJwtSettingsOptions.Object);        

        // Create the controller with real TokenRequestValidatorPipeline instance
        _controller = new CdaTokenController(mockLogger.Object, _mockIdValidator.Object, mockValidatorPipeline.Object, mockScenarioModelRepository.Object, utils);
    }

    [Fact]
    public async Task GenerateTokenAsync_InvalidXRequestId_ReturnsBadRequest()
    {
        // Arrange
        var request = new CdaTokenRequestModel();
        var requestHeader = new RequestHeaderModel { XRequestId = null };  // Invalid XRequestId

        _mockIdValidator.Setup(v => v.IsValidGuid(It.IsAny<string>())).Returns(false);

        // Act
        var result = await _controller.GenerateTokenAsync(request, requestHeader);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(TokenValidationMessages.InvalidXRequestId, badRequestResult.Value);
    }

    [Fact]
    public async Task GenerateTokenAsync_ValidationFailure_ReturnsBadRequest()
    {
        // Arrange
        var request = new CdaTokenRequestModel { GrantType = "Unknown" };
        var requestHeader = new RequestHeaderModel { XRequestId = "valid-guid" };

        _mockIdValidator.Setup(v => v.IsValidGuid(It.IsAny<string>())).Returns(true);

        // Set up a mock validator to return a failure
        var mockValidator = new Mock<ITokenRequestValidator<CdaTokenRequestModel>>();
        mockValidator.Setup(v => v.Validate(It.IsAny<CdaTokenRequestModel>()))
            .Returns(ValidationResult.Failure(TokenValidationMessages.InvalidGrantType));
        
        // Act
        var result = await _controller.GenerateTokenAsync(request, requestHeader);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(TokenValidationMessages.UnsupportedGrantType, badRequestResult.Value);
    }
    
    [Fact]
    public async Task GenerateTokenAsync_ValidationFailure_ReturnsBadRequest_UnsupportedGrantType()
    {
        // Arrange
        var request = new CdaTokenRequestModel { GrantType = "Unknown" };
        var requestHeader = new RequestHeaderModel { XRequestId = "valid-guid" };

        _mockIdValidator.Setup(v => v.IsValidGuid(It.IsAny<string>())).Returns(true);

        // Set up a mock validator to return a failure
        var mockValidator = new Mock<ITokenRequestValidator<CdaTokenRequestModel>>();
        mockValidator.Setup(v => v.Validate(It.IsAny<CdaTokenRequestModel>()))
            .Returns(ValidationResult.Failure(TokenValidationMessages.UnsupportedGrantType));
        
        // Act
        var result = await _controller.GenerateTokenAsync(request, requestHeader);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(TokenValidationMessages.UnsupportedGrantType, badRequestResult.Value);
    }

    [Fact]
    public async Task GenerateTokenAsync_ValidRequest_UmaGrantType_ReturnsOk()
    {
        // Arrange
        var request = new CdaTokenRequestModel
        {
            GrantType = TokenQueryParams.UmaGrantType,
            ClaimToken = TokenQueryParams.ValidJwtToken,
            ClaimTokenFormat = TokenQueryParams.PensionDashboardRqp,
            Scope = TokenQueryParams.Owner,
            Ticket = TokenQueryParams.ValidJwtToken
        };
        var requestHeader = new RequestHeaderModel { XRequestId = "valid-guid" };

        _mockIdValidator.Setup(v => v.IsValidGuid(It.IsAny<string>())).Returns(true);

        // Set up a mock validator to return success
        var mockValidator = new Mock<ITokenRequestValidator<CdaTokenRequestModel>>();
        mockValidator.Setup(v => v.Validate(It.IsAny<CdaTokenRequestModel>()))
            .Returns(ValidationResult.Success());

        // Act
        var result = await _controller.GenerateTokenAsync(request, requestHeader);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<CdaTokenResponseModel>(okResult.Value);
        Assert.NotNull(response.AccessToken);
        Assert.Equal(TokenQueryParams.TokenTypeRpt, response.TokenType);
    }
    
    [Theory]
    [InlineData("PEIS123")]
    [InlineData(Constants.TokenConstants.NullIdTokenCode)]
    [InlineData(Constants.TokenConstants.InvalidIdTokenCode)]
    [InlineData(Constants.TokenConstants.MissingPeisTokenCode)]
    public async Task GenerateTokenAsync_ValidRequest_AuthorizationCodeGrantType_ReturnsOk(string startCode)
    {
        // Arrange
        var request = new CdaTokenRequestModel
        {
            GrantType = TokenQueryParams.AuthorizationCodeGrantType,
            ClientId = TokenQueryParams.ValidClientId,
            ClientSecret = TokenQueryParams.ValidClientSecret,
            Code = TokenQueryParams.ValidCode,
            CodeVerifier = TokenQueryParams.ValidCodeVerifier,
            RedirectUri = Helper.ValidRedirectUri,
        };
        var requestHeader = new RequestHeaderModel { XRequestId = "valid-guid" };

        _mockIdValidator.Setup(v => v.IsValidGuid(It.IsAny<string>())).Returns(true);

        // Set up a mock validator to return success
        var mockValidator = new Mock<ITokenRequestValidator<CdaTokenRequestModel>>();
        mockValidator.Setup(v => v.Validate(It.IsAny<CdaTokenRequestModel>()))
            .Returns(ValidationResult.Success());
        
        
        var testModel = new TokenEmulatorPiesIdScenarioModel
        {
            Id = "1",
            Code = "1",
            PeisIdStartCode = startCode
        };

        var response = new Mock<ItemResponse<TokenEmulatorPiesIdScenarioModel>>();
        response.Setup(r => r.Resource).Returns(testModel);
        
        _mockScenarioModelContainer
            .Setup(c => c.ReadItemAsync<TokenEmulatorPiesIdScenarioModel>(It.IsAny<string>(), It.IsAny<PartitionKey>(), null, default))
            .ReturnsAsync(response.Object); // Mock the ReadItemAsync method

        // Act
        var result = await _controller.GenerateTokenAsync(request, requestHeader);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var responseValue = Assert.IsType<CdaTokenResponseModel>(okResult.Value);
        Assert.NotNull(responseValue.AccessToken);
        if(startCode != Constants.TokenConstants.NullIdTokenCode)
            Assert.NotNull(responseValue.IdToken);
        Assert.Equal(TokenQueryParams.TokenTypeBearer, responseValue.TokenType);
    }
}