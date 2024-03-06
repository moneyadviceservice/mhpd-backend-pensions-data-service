using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PensionProviderIntegrationService.FunctionApp;

namespace PensionProviderIntegrationService.UnitTests
{
    public class FunctionAppUnitTest
    {
        [Fact]
        public void FunctionReturnsCorrectStatusCode()
        {
            var request = new Mock<HttpRequest>();
            var response = Function1.Run(request.Object);

            Assert.True(response.GetType() == typeof(OkObjectResult));

        }
    }
}