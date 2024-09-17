using PensionsRetrievalFunction.Models;
using PensionsRetrievalFunction.Repository;

namespace PensionsRetrievalFunctionTests;

public class PensionRetrievalRepositoryTests
{
    [Fact]
    public async Task WhenRepositoryIsCalledWithMessage_RecordIsSaved()
    {
        //Arrange
        var repository = new PensionRetrievalRepository();
        var message = new PensionRetrievalMessage
        {
            CorrelationId = "Id"
        };

        //Act
        var result = await repository.CreateRecordIfNotExistsAsync(message);

        //Assert
        Assert.False(result);
    }
}
