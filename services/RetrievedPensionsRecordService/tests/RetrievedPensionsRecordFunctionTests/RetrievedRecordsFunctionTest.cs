using MhpdCommon.Constants;
using MhpdCommon.Models.MHPDModels;
using MhpdCommon.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using RetrievedPensionsRecordFunction;
using RetrievedPensionsRecordFunction.Models;
using RetrievedPensionsRecordFunction.Repository;
using System.Net;

namespace RetrievedPensionsRecordFunctionTests;

public class RetrievedRecordsFunctionTest
{
    private readonly Mock<IIdValidator> _idValidatorMock;
    private readonly Mock<ILogger<RetrievedRecordsFunction>> _loggerMock;
    private readonly Mock<IPensionRecordRepository> _repository;
    private readonly RetrievedRecordsFunction _function;

    public RetrievedRecordsFunctionTest()
    {
        _idValidatorMock = new Mock<IIdValidator>();
        _idValidatorMock.Setup(x => x.IsValidGuid(It.IsAny<string>())).Returns(true);

        _loggerMock = new Mock<ILogger<RetrievedRecordsFunction>>();

        _repository = new Mock<IPensionRecordRepository>();
        _repository.Setup(mock => mock.GetRetrievedRecordsAsync(It.IsAny<string>())).ReturnsAsync([new RetrievedPensionRecord()]).Verifiable();

        _function = new RetrievedRecordsFunction(_loggerMock.Object, _repository.Object, _idValidatorMock.Object);
    }

    [Fact]
    public async Task Function_ShouldReturnOk_WhenHeadersAreValid()
    {
        //Arrange
        var retrievalRecordId = Guid.NewGuid().ToString();
        var queryParams = new Dictionary<string, StringValues>
        {
            { Constants.RetrievedRecordQuery, retrievalRecordId}
        };

        var mockRequest = new Mock<HttpRequest>();
        mockRequest.Setup(req => req.Query).Returns(new QueryCollection(queryParams));

        //Act
        var response = await _function.RunAsync(mockRequest.Object);

        //Assert
        var result = Assert.IsType<OkObjectResult>(response);
        Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
        Assert.IsType<List<RetrievedPensionRecord>>(result.Value);
        _repository.Verify(mock => mock.GetRetrievedRecordsAsync(retrievalRecordId), Times.Once);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Function_ShouldReturnBadRequest_WhenHeadersAreInvalid(bool withHeader)
    {
        //Arrange
        _idValidatorMock.Setup(x => x.IsValidGuid(It.IsAny<string>())).Returns(false);
        var queryParams = new Dictionary<string, StringValues>();

        if (withHeader)
            queryParams.Add(HeaderConstants.UserSessionId, Guid.NewGuid().ToString());

        var queries = new QueryCollection(queryParams);
        var mockRequest = new Mock<HttpRequest>();
        mockRequest.Setup(req => req.Query).Returns(queries);

        //Act
        var response = await _function.RunAsync(mockRequest.Object);

        //Assert
        var result = Assert.IsType<BadRequestObjectResult>(response);
        Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Equal(Constants.InvalidRecordId, result.Value);
        _repository.Verify(mock => mock.GetRetrievedRecordsAsync(It.IsAny<string>()), Times.Never);
    }
}
