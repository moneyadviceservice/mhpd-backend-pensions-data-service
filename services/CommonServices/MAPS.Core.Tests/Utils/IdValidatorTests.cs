using MAPS.Core.Utils;

namespace MAPS.Core.Tests.Utils
{
    public class IdValidatorTests
    {
        private readonly IdValidator _idValidator;

        public IdValidatorTests()
        {
            _idValidator = new IdValidator();
        }

        [Theory]
        [InlineData("", false)]
        [InlineData("NotAGuid", false)]
        [InlineData("{1761DC7D-9FD5-47B4-BC77-E3B54AEF0F07}", true)]
        [InlineData("53E30A9D-DB28-4273-8C3D-EDE691C3511A", true)]
        [InlineData("00000000-0000-0000-0000-000000000000", false)]
        [InlineData(null, false)]
        public void WhenAGuidStringIsValidated_ReturnsCorrectResult(string? idString, bool expectedResult)
        {
            var actualResult = _idValidator.IsValidGuid(idString);
            Assert.Equal(expectedResult, actualResult);
        }

        [Theory]
        [InlineData("", false)]
        [InlineData("NotAPei", false)]
        [InlineData("e01a9df7-f147-4a3a-a1dd-0507432a5b7f:1ba03e25-659a-43b8-ae77-b956df168969", true)]
        [InlineData("7075aa11-10ad-4b2f-a9f5-1068e79119bx:1ba03e25-659a-43b8-ae77-b956df168965", false)]
        [InlineData("7075aa11-10ad-4b2f-a9f5-1068e79119b2:1ba03e25-659a-43b8-ae77-b956df16896%", false)]
        [InlineData("00000000-0000-0000-0000-000000000000:53E30A9D-DB28-4273-8C3D-EDE691C3511A:1761DC7D-9FD5-47B4-BC77-E3B54AEF0F07", false)]
        [InlineData(null, false)]
        public void WhenAPeiIsValidated_ReturnsCorrectResult(string? idString, bool expectedResult)
        {
            var actualResult = _idValidator.IsValidPeI(idString);
            Assert.Equal(expectedResult, actualResult);
        }
    }
}