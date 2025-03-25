using System.Net;
using MhpdCommon.Models.Configuration;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Models.MHPDModels;
using MhpdCommon.Models.MHPDModels.JwkUri;
using MhpdCommon.Models.RequestHeaderModel;
using MhpdCommon.Repository;
using MhpdCommon.SharedHttpClient;
using MhpdCommon.TokenValidation;
using MhpdCommon.Utils;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using PensionRequestFunction.HttpClient;
using PensionRequestFunction.Models.CdaPeisServiceClient;
using PensionRequestFunction.Orchestration;
using MapsRqpServiceResponseModel = MhpdCommon.Models.MHPDModels.MapsRqpServiceResponseModel;
using ResponseMessage = MhpdCommon.Models.MHPDModels.ResponseMessage;

namespace PensionRequestFunctionUnitTests;

public class ViewDataOrchestratorUnitTests
{
    private ViewDataOrchestrator _orchestrator;

    private readonly Mock<IMapsCdaServiceClient> _mockMapsRqpService = new();
    private readonly Mock<ITokenUtility> _tokenUtility = new();
    private readonly Mock<ITokenIntegrationServiceClient> _mockTokenIntegrationService = new();
    private readonly Mock<IPdpViewDataClient> _mockPdpViewDataClient = new();
    private readonly Mock<IHolderNameClient> _holderNameClient = new();
    private readonly Mock<ViewDataOrchestratorClients> _mockServiceClients;
    private readonly Mock<IIdValidator> _validator = new();
    private readonly Mock<ILogger<ViewDataOrchestrator>> _logger = new();
    private readonly Mock<Container> _mockUserSessionDataContainer;
    private readonly Mock<UserSessionDataRepository> _mockUserSessionDataRepository;

    public ViewDataOrchestratorUnitTests()
    {
        var responseHeader = GetResponseHeader();
        var rqp = GetRqp();
        var rpt = GetRpt();

        _mockPdpViewDataClient.Setup(x => x.GetPdpViewDataAsync(It.IsAny<string>(), It.IsAny<string>(), It.Is<string>(x => string.IsNullOrEmpty(x)), It.IsAny<string>()))
            .ReturnsAsync(new PdpServiceResponseModel { 
                ViewDataToken = null, 
                ResponseMessage = new ResponseMessage { 
                    ResponseStatusCode = HttpStatusCode.Unauthorized, 
                    WwwAuthenticateResponseHeader = responseHeader 
                }
            });

        _mockPdpViewDataClient.Setup(x => x.GetPdpViewDataAsync(It.IsAny<string>(), It.IsAny<string>(), It.Is<string>(x => !string.IsNullOrEmpty(x)), It.IsAny<string>()))
            .ReturnsAsync(new PdpServiceResponseModel
            {
                ViewDataToken = GetViewDataToken(ViewDataToken),
                ResponseMessage = new ResponseMessage { ResponseStatusCode = HttpStatusCode.OK }
            });

        _mockMapsRqpService.Setup(x => x.PostRqp(It.IsAny<RequestHeaderModel>()))
            .ReturnsAsync(new MapsRqpServiceResponseModel { Rqp = rqp });

        _mockTokenIntegrationService.Setup(x => x.PostRptAsync(It.IsAny<TokenClientRequestModel>()))
            .ReturnsAsync(new CdaTokenResponseModel {  AccessToken = rpt });

        var holderNameId = Guid.NewGuid().ToString();
        var assetId = Guid.NewGuid().ToString();
        _validator.Setup(mock => mock.IsValidGuid(It.IsAny<string>())).Returns(true);
        _validator.Setup(mock => mock.TryExtractPei(It.IsAny<string>(), out holderNameId, out assetId)).Returns(true);

        var parser = new Mock<IMessageParser>();
        parser.Setup(mock => mock.ToPensionRequestPayload(It.IsAny<string>())).Returns(new PensionRequestPayload());

        _holderNameClient.Setup(x => x.GetViewDataUrlAsync(holderNameId, It.IsAny<string>())).ReturnsAsync(new HolderNameConfigurationModel
        {
            ViewDataUrl = "https://viewdata.com"
        });

        _tokenUtility.Setup(m => m.RetrieveClaim(It.IsAny<string>(), It.IsAny<string>())).Returns("viewDataClaim");

        var jwtUtilityMock = new Mock<IJwtUtility>();

        // Create an instance of PensionServiceClients with mocked dependencies
        _mockServiceClients = new Mock<ViewDataOrchestratorClients>(
            _holderNameClient.Object,
            _mockPdpViewDataClient.Object,
            _mockTokenIntegrationService.Object,
            _mockMapsRqpService.Object
        );
        
        Mock<CosmosClient> mockCosmosClient = new();
        _mockUserSessionDataContainer = new Mock<Container>();
        Mock<Database> mockDatabase = new();
        
        var configuration = new CosmosBusinessConfiguration
        {
            DatabaseId = "TestDatabase",
            UserSessionDataContainer = "TestUserSessionDataContainer",
        };
        
        Mock<IOptions<CosmosBusinessConfiguration>> mockCosmosConfigOptions = new();
        mockCosmosConfigOptions.Setup(x => x.Value).Returns(configuration);

        mockCosmosClient.Setup(mock => mock.GetDatabase(configuration.DatabaseId))
            .Returns(mockDatabase.Object);

        mockDatabase.Setup(mock => mock.GetContainer(configuration.UserSessionDataContainer))
            .Returns(_mockUserSessionDataContainer.Object);

        // Instantiate the _mockUserSessionDataRepository with the mocked CosmosClient and configuration
        _mockUserSessionDataRepository =
            new Mock<UserSessionDataRepository>(mockCosmosClient.Object, mockCosmosConfigOptions.Object);
        
        var testInstanceData = new UserSessionData
        {
            UserSessionId = Guid.NewGuid().ToString(),
            Pct = TokenQueryParams.ValidPersistedClaimsToken
        };
        
        var mockItemResponse = new Mock<ItemResponse<UserSessionData>>();
        mockItemResponse.Setup(x => x.Resource).Returns(testInstanceData);
        
        _mockUserSessionDataContainer.Setup(x => x.ReadItemAsync<UserSessionData>(It.IsAny<string>(), It.IsAny<PartitionKey>(), null, default))
            .ReturnsAsync(mockItemResponse.Object);
        
        _orchestrator = new ViewDataOrchestrator(
            _logger.Object, 
            _validator.Object,
            _tokenUtility.Object,
            jwtUtilityMock.Object,
            _mockServiceClients.Object,
            _mockUserSessionDataRepository.Object);
    }

    [Fact]
    public async Task WhenViewDataIsInvoked_DoesAuthDanceAndReturnsViewData()
    {
        // Act
        var viewData = await _orchestrator.GetPensionViewDataAsync(Pei, Iss, UserSessionId, CorrelationId);

        // Assert
        Assert.NotNull(viewData);
    }

    [Theory]
    [InlineData(JwkConstants.TestTokenNoKid)]
    [InlineData(JwkConstants.TestTokenUnknownKid)]
    [InlineData(JwkConstants.TestTokenExpired)]
    [InlineData(JwkConstants.TestTokenUnknownKey)]
    public async Task WhenViewDataTokenIsInvalid_ThrowsException(string viewDataToken)
    {
        // Arrange
        _mockPdpViewDataClient.Setup(x => x.GetPdpViewDataAsync(It.IsAny<string>(), It.IsAny<string>(), It.Is<string>(x => !string.IsNullOrEmpty(x)), It.IsAny<string>()))
            .ReturnsAsync(new PdpServiceResponseModel
            {
                ViewDataToken = GetViewDataToken(viewDataToken),
                ResponseMessage = new ResponseMessage { ResponseStatusCode = HttpStatusCode.Accepted }
            });

        var client = new Mock<ISharedHttpClient>();
        client.Setup(mock => mock.GetAsync()).ReturnsAsync(JwkUriResponseModel.Default);

        _orchestrator = new ViewDataOrchestrator(      
            _logger.Object,
            _validator.Object,
            _tokenUtility.Object,
            new JwtUtility(client.Object),
            _mockServiceClients.Object,
            _mockUserSessionDataRepository.Object
            );

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _orchestrator.GetPensionViewDataAsync(Pei, Iss, UserSessionId, CorrelationId));
    }

    private const string CorrelationId = "e01a9df7-f147-4a3a-a1dd-0507432a5b7f";
    private const string Pei = "7075aa11-10ad-4b2f-a9f5-1068e79119bf:1ba03e25-659a-43b8-ae77-b956df168969";
    private const string Iss = "DATA_PROVIDER_1fd1da88-9fb3-461c-a48a-3dba21bfba17";
    private const string UserSessionId = "459566f6-5fce-479e-a098-298ca9676a85";
    private const string ViewDataToken = "eyJhbGciOiJSUzI1NiIsImtpZCI6ImMwMGI0MGVhLTZkYTEtNDA4YS1hNmM5LTE3YjFmZjQ1YmI5YSIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIzMjRicWZ3MzQ4ZjlxNDM5OGgzIiwiaWF0IjoxNzIzMTI5NTM4LCJleHAiOjE3MjMxMjk1OTgsImp0aSI6IjA0OTczYTFkLTQ1ZGMtNGIxYy1iYTE3LTJjM2NiYmNiNGYwMSIsImF1ZCI6Imh0dHBzOi8vcGRwL2lnL3Rva2VuIiwiaXNzIjoiREFUQV9QUk9WSURFUl8xZmQxZGE4OC05ZmIzLTQ2MWMtYTQ4YS0zZGJhMjFiZmJhMTciLCJ2aWV3X2RhdGEiOiJ7XCJhcnJhbmdlbWVudHNcIjpbe1wicGVuc2lvblByb3ZpZGVyU2NoZW1lTmFtZVwiOlwiU3RhdGUgUGVuc2lvblwiLFwicG9zc2libGVNYXRjaFwiOmZhbHNlLFwicGVuc2lvblR5cGVcIjpcIlNQXCIsXCJzdGF0ZVBlbnNpb25EYXRlXCI6XCIyMDQyLTAyLTIzXCIsXCJwZW5zaW9uQWRtaW5pc3RyYXRvclwiOntcIm5hbWVcIjpcIkRXUFwiLFwiY29udGFjdE1ldGhvZHNcIjpbe1wicHJlZmVycmVkXCI6ZmFsc2UsXCJjb250YWN0TWV0aG9kRGV0YWlsc1wiOntcInBvc3RhbE5hbWVcIjpcIkZyZWVwb3N0IERXUFwiLFwibGluZTFcIjpcIlBlbnNpb25zIFNlcnZpY2UgM1wiLFwiY291bnRyeUNvZGVcIjpcIkdCXCJ9fSx7XCJwcmVmZXJyZWRcIjp0cnVlLFwiY29udGFjdE1ldGhvZERldGFpbHNcIjp7XCJ1cmxcIjpcImh0dHBzOi8vd3d3Lmdvdi51ay9mdXR1cmUtcGVuc2lvbi1jZW50cmVcIn19LHtcInByZWZlcnJlZFwiOmZhbHNlLFwiY29udGFjdE1ldGhvZERldGFpbHNcIjp7XCJudW1iZXJcIjpcIis0NCA4MDA3MzEwMTc1XCIsXCJ1c2FnZVwiOltcIk1cIixcIldcIl19fSx7XCJwcmVmZXJyZWRcIjpmYWxzZSxcImNvbnRhY3RNZXRob2REZXRhaWxzXCI6e1wibnVtYmVyXCI6XCIrNDQgODAwNzMxMDE3NlwiLFwidXNhZ2VcIjpbXCJNXCJdfX0se1wicHJlZmVycmVkXCI6ZmFsc2UsXCJjb250YWN0TWV0aG9kRGV0YWlsc1wiOntcIm51bWJlclwiOlwiKzQ0IDgwMDczMTA0NTZcIixcInVzYWdlXCI6W1wiV1wiXX19LHtcInByZWZlcnJlZFwiOmZhbHNlLFwiY29udGFjdE1ldGhvZERldGFpbHNcIjp7XCJudW1iZXJcIjpcIis0NCAxOTEyMTgyMDUxXCIsXCJ1c2FnZVwiOltcIk5cIl19fSx7XCJwcmVmZXJyZWRcIjpmYWxzZSxcImNvbnRhY3RNZXRob2REZXRhaWxzXCI6e1wibnVtYmVyXCI6XCIrNDQgMTkxMjE4MzYwMFwiLFwidXNhZ2VcIjpbXCJOXCJdfX1dfSxcImJlbmVmaXRJbGx1c3RyYXRpb25zXCI6W3tcImlsbHVzdHJhdGlvbkNvbXBvbmVudHNcIjpbe1wiaWxsdXN0cmF0aW9uVHlwZVwiOlwiRVJJXCIsXCJjYWxjdWxhdGlvbk1ldGhvZFwiOlwiQlNcIixcInBheWFibGVEZXRhaWxzXCI6e1wicGF5YWJsZURhdGVcIjpcIjIwNDItMDItMjNcIixcImFubnVhbEFtb3VudFwiOjExNTAyLFwibW9udGhseUFtb3VudFwiOjk1OC41LFwiYW1vdW50VHlwZVwiOlwiSU5DXCIsXCJpbmNyZWFzaW5nXCI6dHJ1ZX19LHtcImlsbHVzdHJhdGlvblR5cGVcIjpcIkFQXCIsXCJjYWxjdWxhdGlvbk1ldGhvZFwiOlwiQlNcIixcInBheWFibGVEZXRhaWxzXCI6e1wicGF5YWJsZURhdGVcIjpcIjIwNDItMDItMjNcIixcImFubnVhbEFtb3VudFwiOjExNTAyLFwibW9udGhseUFtb3VudFwiOjk1OC41LFwiYW1vdW50VHlwZVwiOlwiSU5DXCIsXCJpbmNyZWFzaW5nXCI6dHJ1ZX19XSxcImlsbHVzdHJhdGlvbkRhdGVcIjpcIjIwMjQtMDgtMjRcIn1dLFwiYWRkaXRpb25hbERhdGFTb3VyY2VzXCI6W3tcInVybFwiOlwiaHR0cHM6Ly93d3cuZ292LnVrL2NoZWNrLXN0YXRlLXBlbnNpb25cIixcImluZm9ybWF0aW9uVHlwZVwiOlwiU1BcIn1dLFwic3RhdGVQZW5zaW9uTWVzc2FnZUVuZ1wiOlwiU3RhdGUgcGVuc2lvbiBtZXNzYWdlIGluIEVuZ2xpc2guXCIsXCJzdGF0ZVBlbnNpb25NZXNzYWdlV2Vsc2hcIjpcIk5lZ2VzIHBlbnNpd24gZ3dsYWRvbCB5biBTYWVzbmVnLlwifV19IiwibmJmIjoxNzIzMTI5NTM4fQ.svwxVmzgcRMPEL-gJnv8tFu-FxwiPJuhwcUKgF4MS_9ExKPR_NzkFPsKY_5NFzG2H83Dr6Njfy9WuMYkOoN139SfL8yKwjlfotRQeSfieZzaItX15hYKtJFnKmhPle2AAFGSUcdwjwtBvlssbJQkFswypWXDoUMinKsqBRaU8YbxKSWZQjboZy-2FYk7ORAP2oqaAVS9RgSmGv_hoZmL3kYF7ZjzEYzLq4rJ6gHISOjxL2s_tDX7Q9RlZudrG_rCWCTfuYoc_-IE5ucVvlfr35eKZDRM1pHUo45EZKS1cps-4u7QWA_qrtjc9XNG2N-xiIaZY4epNKTb8o7LWkn9_A";

    private static string GetViewDataToken(string viewDataToken)
    {
        return "{\"view_data_token\":\"" + viewDataToken + "\"}";
    }

    private static string GetResponseHeader()
    {
        return "realm=\"PensionDashboard\", as_uri=\"https://as.pdp.com\", ticket=\"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.cThIIoDvwdueQB468K5xDc5633seEFoqwxjF_xSJyQQ\"";
    }

    private static string GetRqp()
    {
        return "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
    }

    private static string GetRpt ()
    {
        return "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2dnZWRJbkFzIjoiYWRtaW4iLCJpYXQiOjE0MjI3Nzk2Mzh9.gzSraSYS8EXBxLN_oWnFSRgCzcmJmMjLiuyu5CSpyHI";
    }
}
