using MhpdCommon.Constants;
using MhpdCommon.Utils;

namespace MhpdCommonTests.Utils
{
    public class ResourceProviderTest
    {
        [Fact]
        public void WhenGetStringIsCalled_ReturnsTheFileContents()
        {
            var contents = ResourceProvider.GetString(FileConstants.RetrievedPensionPayloadSchema);
            Assert.False(string.IsNullOrWhiteSpace(contents));
        }

        [Fact]
        public void WhenGetStreamIsCalled_ReturnsTheFileStream()
        {
            var stream = ResourceProvider.GetStream(FileConstants.RetrievedPensionRecordSchema);
            Assert.True(stream != null && stream.Length > 0);
        }

        [Fact]
        public void WhenGetStringIsCalled_ThrowsAnError()
        {
            static string? act() => ResourceProvider.GetString("Schemas.DoesNotExist.json");

            var error = Record.Exception((Func<string?>)act);
            Assert.NotNull(error);
            Assert.IsType<InvalidOperationException>(error);
        }

        [Fact]
        public void WhenGetStreamIsCalled_ThrowsAnError()
        {
            static Stream? act() => ResourceProvider.GetStream("Schemas.DoesNotExist.json");

            var error = Record.Exception((Func<Stream?>)act);
            Assert.NotNull(error);
            Assert.IsType<InvalidOperationException>(error);
        }
    }
}
