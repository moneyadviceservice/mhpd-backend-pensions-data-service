using MhpdCommon.Constants;
using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Utils;
using Moq;
using PensionRequestFunction.Constants;
using PensionRequestFunction.Transformer;
using System.Text.Json;

namespace PensionRequestFunctionUnitTests;

public class ViewDataToPensionArrangementTransformerTests
{
    private ViewDataToPensionArrangementTransformer _transformer;
    private readonly Mock<IIdValidator> _idValidator;
    private static readonly MessageParser _messageParser = new();

    public ViewDataToPensionArrangementTransformerTests()
    {
        _idValidator = new Mock<IIdValidator>();
        _idValidator.Setup(mock => mock.IsValidGuid(It.IsAny<string>())).Returns(true);
        _transformer = new ViewDataToPensionArrangementTransformer(_idValidator.Object);
    }

    [Fact]
    public void Transform_ValidJson_TransformsSuccessfully()
    {
        // Arrange
        var externalAssetId = Guid.NewGuid().ToString();
        var pei = $"{Guid.NewGuid()}:{Guid.NewGuid()}";
        var retrievalRecordId = Guid.NewGuid().ToString();
        var validJson = GetViewDataPayload();

        // Act
        var result = _transformer.Transform(externalAssetId, validJson, pei, retrievalRecordId);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("Your Pension DC Master Trust", result);
        Assert.Contains(externalAssetId, result);
        Assert.Contains(pei, result);
        Assert.Contains(retrievalRecordId, result);
        Assert.NotNull(Parse(result));
    }

    [Fact]
    public void Transform_FailedViewData_TransformsSuccessfully()
    {
        // Arrange
        var errorCode = PensionProviderConstants.RetrievalErrorCodes.SystemError;
        var pei = $"{Guid.NewGuid()}:{Guid.NewGuid()}";
        var retrievalRecordId = Guid.NewGuid().ToString();

        // Act
        var result = _transformer.Transform(errorCode, pei, retrievalRecordId);

        // Assert
        Assert.NotNull(result);
        Assert.Contains(errorCode, result);
        Assert.Contains(pei, result);
        Assert.Contains(retrievalRecordId, result);
        Assert.NotNull(Parse(result));
    }

    [Fact]
    public void Transform_EmptyJson_ThrowsException()
    {
        // Arrange
        var externalAssetId = Guid.NewGuid().ToString();
        var pei = $"{Guid.NewGuid}:{Guid.NewGuid}";
        var retrievalRecordId = Guid.NewGuid().ToString();
        var emptyJson = string.Empty;

        // Act & Assert
        var ex = Assert.Throws<InvalidDataException>(() => _transformer.Transform(externalAssetId, emptyJson, pei, retrievalRecordId));
        Assert.Equal("No arrangements present", ex.Message);
    }

    [Fact]
    public void Transform_InvalidExternalAssetId_ThrowsFormatException()
    {
        // Arrange
        var invalidExternalAssetId = "invalid-guid";
        var pei = $"{Guid.NewGuid()}:{Guid.NewGuid()}";
        var retrievalRecordId = Guid.NewGuid().ToString();
        var validJson = GetViewDataPayload();
        _idValidator.Setup(mock => mock.IsValidGuid(invalidExternalAssetId)).Returns(false);
        _transformer  = new ViewDataToPensionArrangementTransformer(_idValidator.Object);

        // Act & Assert
        var ex = Assert.Throws<InvalidDataException>(() => _transformer.Transform(invalidExternalAssetId, validJson, pei, retrievalRecordId));
        Assert.Equal(StatusConstants.InvalidExternalAssetId, ex.Message);
    }

    [Fact]
    public void Transform_NullArrangements_ThrowsException()
    {
        // Arrange
        var externalAssetId = Guid.NewGuid().ToString();
        var pei = $"{Guid.NewGuid}:{Guid.NewGuid}";
        var retrievalRecordId = Guid.NewGuid().ToString();
        var nullArrangementsJson = "{\"arrangements\": null}";

        // Act & Assert
        var ex = Assert.Throws<JsonException>(() => _transformer.Transform(externalAssetId, nullArrangementsJson, pei, retrievalRecordId));
        Assert.Equal("The payload either lacks the 'arrangements' property, or the property is not a valid array.", ex.Message);
    }

    [Fact]
    public void Transform_EmptyArrangements_ThrowsException()
    {
        // Arrange
        var externalAssetId = Guid.NewGuid().ToString();
        var pei = $"{Guid.NewGuid}:{Guid.NewGuid}";
        var retrievalRecordId = Guid.NewGuid().ToString();
        var emptyArrangementsJson = "{\"arrangements\": []}";

        // Act & Assert
        var ex = Assert.Throws<JsonException>(() => _transformer.Transform(externalAssetId, emptyArrangementsJson, pei, retrievalRecordId));
        Assert.Equal("The payload either lacks the 'arrangements' property, or the property is not a valid array.", ex.Message);
    }

    [Fact]
    public void Transform_AlternateSchemeNamesPresent_AddsToJson()
    {
        // Arrange
        var externalAssetId = Guid.NewGuid().ToString();
        var pei = $"{Guid.NewGuid()}:{Guid.NewGuid()}";
        var retrievalRecordId = Guid.NewGuid().ToString();
        var jsonWithAlternateSchemeNames = GetModifiedViewDataPayload();

        // Act
        var result = _transformer.Transform(externalAssetId, jsonWithAlternateSchemeNames, pei, retrievalRecordId);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("ABC", result);
        Assert.NotNull(Parse(result));
    }

    [Fact]
    public void Transform_FailedPensionRetrieval_ReturnsErrorCode()
    {
        // Arrange
        var errorCode = PensionProviderConstants.RetrievalErrorCodes.SystemError;
        var pei = $"{Guid.NewGuid()}:{Guid.NewGuid()}";
        var retrievalRecordId = Guid.NewGuid().ToString();

        // Act
        var result = _transformer.Transform(errorCode, pei, retrievalRecordId);

        // Assert
        Assert.NotNull(result);
        Assert.Contains(errorCode, result);
        Assert.NotNull(Parse(result));
    }

    private static RetrievedPensionDetailsPayload? Parse(string transformOutput)
    {
        return _messageParser.ToRetrievedPensionPayload(transformOutput);
    }
    
    private string GetViewDataPayload()
    {
        return "{\"arrangements\":[{\"pensionProviderSchemeName\":\"Your Pension DC Master Trust\",\"possibleMatchReference\":\"D1006548723\",\"pensionType\":\"DC\",\"pensionOrigin\":\"WM\",\"pensionStatus\":\"A\",\"pensionStartDate\":\"1998-05-16\",\"retirementDate\":\"2038-09-18\",\"dateOfBirth\":\"1973-09-18\",\"possibleMatch\":false,\"pensionAdministrator\":{\"name\":\"Your Pension\",\"contactMethods\":[{\"preferred\":false,\"contactMethodDetails\":{\"email\":\"mastertrust@yourpension.com\"}},{\"preferred\":true,\"contactMethodDetails\":{\"url\":\"https://www.yourpension.co.uk\"}},{\"preferred\":false,\"contactMethodDetails\":{\"number\":\"+44 80080087355\",\"usage\":[\"M\"]}},{\"preferred\":false,\"contactMethodDetails\":{\"postalName\":\"Your Pension\",\"line1\":\"92 Victoria Lane\",\"line2\":\"Frampton Cotterell\",\"line3\":\"Bristol\",\"line4\":\"South Glocustershire\",\"postcode\":\"BS36 9DD\",\"countryCode\":\"GB\"}}]},\"employmentMembershipPeriods\":[{\"employerName\":\"Sweets R Us\",\"employerStatus\":\"C\",\"membershipStartDate\":\"1998-05-16\"}],\"benefitIllustrations\":[{\"illustrationComponents\":[{\"benefitType\":\"DC\",\"calculationMethod\":\"SMPI\",\"payableDetails\":{\"payableDate\":\"2038-09-18\",\"increasing\":false,\"monthlyAmount\":1725,\"annualAmount\":20700,\"amountType\":\"INC\"},\"estimatedDcPot\":300000,\"survivorBenefit\":false,\"safeguardedBenefit\":false},{\"amountType\":\"INC\",\"benefitType\":\"DC\",\"calculationMethod\":\"SMPI\",\"payableDetails\":{\"payableDate\":\"2038-09-18\",\"increasing\":false,\"monthlyAmount\":1351,\"annualAmount\":16215,\"amountType\":\"INC\"},\"accruedDcPot\":235000,\"survivorBenefit\":false,\"safeguardedBenefit\":false}],\"illustrationDate\":\"2023-05-16\"}]}]}";
    }
    private string GetViewDataPayloadPOSS()
    {
        return "{\r\n\t\"arrangements\": [{\"pensionProviderSchemeName\":\"Your Pension DC Master Trust\",\"possibleMatchReference\":\"D1006548723\",\"pensionType\":\"DC\",\"pensionOrigin\":\"WM\",\"pensionStatus\":\"A\",\"pensionStartDate\":\"1998-05-16\",\"retirementDate\":\"2038-09-18\",\"dateOfBirth\":\"1973-09-18\",\"possibleMatch\":true,\"pensionAdministrator\":{\"name\":\"Your Pension\",\"contactMethods\":[{\"preferred\":false,\"contactMethodDetails\":{\"email\":\"mastertrust@yourpension.com\"}},{\"preferred\":true,\"contactMethodDetails\":{\"url\":\"https://www.yourpension.co.uk\"}},{\"preferred\":false,\"contactMethodDetails\":{\"number\":\"+44 80080087355\",\"usage\":[\"M\"]}},{\"preferred\":false,\"contactMethodDetails\":{\"postalName\":\"Your Pension\",\"line1\":\"92 Victoria Lane\",\"line2\":\"Frampton Cotterell\",\"line3\":\"Bristol\",\"line4\":\"South Glocustershire\",\"postcode\":\"BS36 9DD\",\"countryCode\":\"GB\"}}]},\"employmentMembershipPeriods\":[{\"employerName\":\"Sweets R Us\",\"employerStatus\":\"C\",\"employmentStartDate\":\"1998-05-16\"}],\"benefitIllustrations\":[{\"illustrationComponents\":[{\"illustrationType\":\"ERI\",\"benefitType\":\"DC\",\"calculationMethod\":\"SMPI\",\"payableDetails\":{\"payableDate\":\"2038-09-18\",\"annualAmount\":20700,\"amountType\":\"INC\"},\"dcPot\":300000,\"survivorBenefit\":false,\"safeguardedBenefit\":false},{\"illustrationType\":\"AP\",\"benefitType\":\"DC\",\"calculationMethod\":\"SMPI\",\"payableDetails\":{\"payableDate\":\"2038-09-18\",\"annualAmount\":16215,\"amountType\":\"INC\"},\"dcPot\":235000,\"survivorBenefit\":false,\"safeguardedBenefit\":false}],\"illustrationDate\":\"2023-05-16\"}]}]\r\n}";
    }
    private string GetModifiedViewDataPayload()
    {
        return "{\"arrangements\":[{\"pensionProviderSchemeName\":\"ABC\",\"possibleMatchReference\":\"D9999\",\"pensionType\":\"SP\",\"pensionOrigin\":\"PC\",\"pensionStatus\":\"PC\",\"pensionStartDate\":\"2024-05-05\",\"retirementDate\":\"2042-05-05\",\"dateOfBirth\":\"2000-05-05\",\"possibleMatch\":true,\"pensionAdministrator\":{\"name\":\"ABC Your Pension\",\"contactMethods\":[{\"preferred\":false,\"contactMethodDetails\":{\"email\":\"abcmastertrust@yourpension.com\"}},{\"preferred\":true,\"contactMethodDetails\":{\"url\":\"https://www.abcyourpension.co.uk\"}},{\"preferred\":false,\"contactMethodDetails\":{\"number\":\"+44 9999999999\",\"usage\":[\"A\"]}},{\"preferred\":false,\"contactMethodDetails\":{\"postalName\":\"ABCYour Pension\",\"line1\":\"92 Victoria Lane\",\"line2\":\"Frampton Cotterell\",\"line3\":\"Bristol\",\"line4\":\"South Glocustershire\",\"postcode\":\"BS36 9DD\",\"countryCode\":\"GB\"}}]},\"employmentMembershipPeriods\":[{\"employerName\":\"ABCSweets R Us\",\"employerStatus\":\"H\",\"employmentStartDate\":\"1998-05-16\"}],\"benefitIllustrations\":[{\"illustrationComponents\":[{\"benefitType\":\"DC\",\"calculationMethod\":\"SMPI\",\"payableDetails\":{\"payableDate\":\"2038-09-18\",\"annualAmount\":20700,\"amountType\":\"INC\"},\"estimatedDcPot\":300000,\"survivorBenefit\":false,\"safeguardedBenefit\":false},{\"benefitType\":\"DC\",\"calculationMethod\":\"SMPI\",\"payableDetails\":{\"payableDate\":\"2038-09-18\",\"annualAmount\":16215,\"amountType\":\"INC\"},\"accruedDcPot\":235000,\"survivorBenefit\":false,\"safeguardedBenefit\":false}],\"illustrationDate\":\"2030-05-05\"}]}]}";

    }
}