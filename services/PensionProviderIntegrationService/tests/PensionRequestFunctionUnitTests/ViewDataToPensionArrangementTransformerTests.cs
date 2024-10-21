using System.Text.Json;
using PensionRequestFunction.Transformer;

namespace PensionRequestFunctionUnitTests;

public class ViewDataToPensionArrangementTransformerTests
{
    private readonly ViewDataToPensionArrangementTransformer _transformer = new();

    [Fact]
    public void Transform_ValidJson_TransformsSuccessfully()
    {
        // Arrange
        var externalAssetId = Guid.NewGuid().ToString();
        var pei = $"{Guid.NewGuid}:{Guid.NewGuid}";
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
        var ex = Assert.Throws<Exception>(() => _transformer.Transform(externalAssetId, emptyJson, pei, retrievalRecordId));
        Assert.Equal("No arrangements present", ex.Message);
    }

    [Fact]
    public void Transform_InvalidExternalAssetId_ThrowsFormatException()
    {
        // Arrange
        var invalidExternalAssetId = "invalid-guid";
        var pei = $"{Guid.NewGuid}:{Guid.NewGuid}";
        var retrievalRecordId = Guid.NewGuid().ToString();
        var validJson = GetViewDataPayload();

        // Act & Assert
        var ex = Assert.Throws<Exception>(() => _transformer.Transform(invalidExternalAssetId, validJson, pei, retrievalRecordId));
        Assert.Equal("Invalid externalAssetId. It must be a valid GUID.", ex.Message);
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
        var pei = $"{Guid.NewGuid}:{Guid.NewGuid}";
        var retrievalRecordId = Guid.NewGuid().ToString();
        var jsonWithAlternateSchemeNames = GetModifiedViewDataPayload();

        // Act
        var result = _transformer.Transform(externalAssetId, jsonWithAlternateSchemeNames, pei, retrievalRecordId);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("ABC", result);
    }

    [Fact]
    public void Transform_MissingRetirementDate_ThrowsException()
    {
        // Arrange
        var externalAssetId = Guid.NewGuid().ToString();
        var pei = $"{Guid.NewGuid}:{Guid.NewGuid}";
        var retrievalRecordId = Guid.NewGuid().ToString();
        var jsonMissingRetirementDate = "{\"arrangements\": [{\"pensionProviderSchemeName\": \"Test Scheme\"}]}";

        // Act & Assert
        var ex = Assert.Throws<Exception>(() => _transformer.Transform(externalAssetId, jsonMissingRetirementDate, pei, retrievalRecordId));
        Assert.Equal("MatchType not found", ex.Message);
    }

    [Fact]
    public void Transform_MissingMatchType_ThrowsException()
    {
        // Arrange
        var externalAssetId = Guid.NewGuid().ToString();
        var pei = $"{Guid.NewGuid}:{Guid.NewGuid}";
        var retrievalRecordId = Guid.NewGuid().ToString();
        var jsonMissingMatchType = "{\"arrangements\": [{\"pensionProviderSchemeName\": \"Test Scheme\", \"retirementDate\": \"2025-01-01\"}]}";

        // Act & Assert
        var ex = Assert.Throws<Exception>(() => _transformer.Transform(externalAssetId, jsonMissingMatchType, pei, retrievalRecordId));
        Assert.Equal("MatchType not found", ex.Message);
    }
    
    private string GetViewDataPayload()
    {
        return "{\r\n\t\"arrangements\": [{\"pensionProviderSchemeName\":\"Your Pension DC Master Trust\",\"possibleMatchReference\":\"D1006548723\",\"pensionType\":\"DC\",\"pensionOrigin\":\"WM\",\"pensionStatus\":\"A\",\"pensionStartDate\":\"1998-05-16\",\"retirementDate\":\"2038-09-18\",\"dateOfBirth\":\"1973-09-18\",\"possibleMatch\":false,\"pensionAdministrator\":{\"name\":\"Your Pension\",\"contactMethods\":[{\"preferred\":false,\"contactMethodDetails\":{\"email\":\"mastertrust@yourpension.com\"}},{\"preferred\":true,\"contactMethodDetails\":{\"url\":\"https://www.yourpension.co.uk\"}},{\"preferred\":false,\"contactMethodDetails\":{\"number\":\"+44 80080087355\",\"usage\":[\"M\"]}},{\"preferred\":false,\"contactMethodDetails\":{\"postalName\":\"Your Pension\",\"line1\":\"92 Victoria Lane\",\"line2\":\"Frampton Cotterell\",\"line3\":\"Bristol\",\"line4\":\"South Glocustershire\",\"postcode\":\"BS36 9DD\",\"countryCode\":\"GB\"}}]},\"employmentMembershipPeriods\":[{\"employerName\":\"Sweets R Us\",\"employerStatus\":\"C\",\"membershipStartDate\":\"1998-05-16\"}],\"benefitIllustrations\":[{\"illustrationComponents\":[{\"illustrationType\":\"ERI\",\"benefitType\":\"DC\",\"calculationMethod\":\"SMPI\",\"payableDetails\":{\"payableDate\":\"2038-09-18\", \"increasing\": false,\"monthlyAmount\": 1725,\"annualAmount\":20700,\"amountType\":\"INC\"},\"dcPot\":300000,\"survivorBenefit\":false,\"safeguardedBenefit\":false},{\"illustrationType\":\"AP\",\"amountType\": \"INC\",\"benefitType\":\"DC\",\"calculationMethod\":\"SMPI\",\"payableDetails\":{\"payableDate\":\"2038-09-18\",\"increasing\": false,\"monthlyAmount\": 1351, \"annualAmount\":16215,\"amountType\":\"INC\"},\"dcPot\":235000,\"survivorBenefit\":false,\"safeguardedBenefit\":false}],\"illustrationDate\":\"2023-05-16\"}]}]\r\n}";
    }
    
    private string GetRequestPayloadWithAssetID99a9b3c9()
    {
        return "{\r\n\t\"pensionRetrievalRecordId\": \"e01a9df7-f147-4a3a-a1dd-0507432a5b7f\",\r\n\t\"pei\": \"7075aa11-10ad-4b2f-a9f5-1068e79119bf:1ba03e25-659a-43b8-ae77-b956df168969\",\r\n\t\"iss\": \"DATA_PROVIDER_1fd1da88-9fb3-461c-a48a-3dba21bfba17\",\r\n\t\"userSessionId\": \"99a9b3c9-ac18-43c3-b2e7-723a74eba292\",\r\n\t\"asset_guid\": \"99a9b3c9-ac18-43c3-b2e7-723a74eba292\"\r\n}";
    }
    private string GetViewDataPayloadPOSS()
    {
        return "{\r\n\t\"arrangements\": [{\"pensionProviderSchemeName\":\"Your Pension DC Master Trust\",\"possibleMatchReference\":\"D1006548723\",\"pensionType\":\"DC\",\"pensionOrigin\":\"WM\",\"pensionStatus\":\"A\",\"pensionStartDate\":\"1998-05-16\",\"retirementDate\":\"2038-09-18\",\"dateOfBirth\":\"1973-09-18\",\"possibleMatch\":true,\"pensionAdministrator\":{\"name\":\"Your Pension\",\"contactMethods\":[{\"preferred\":false,\"contactMethodDetails\":{\"email\":\"mastertrust@yourpension.com\"}},{\"preferred\":true,\"contactMethodDetails\":{\"url\":\"https://www.yourpension.co.uk\"}},{\"preferred\":false,\"contactMethodDetails\":{\"number\":\"+44 80080087355\",\"usage\":[\"M\"]}},{\"preferred\":false,\"contactMethodDetails\":{\"postalName\":\"Your Pension\",\"line1\":\"92 Victoria Lane\",\"line2\":\"Frampton Cotterell\",\"line3\":\"Bristol\",\"line4\":\"South Glocustershire\",\"postcode\":\"BS36 9DD\",\"countryCode\":\"GB\"}}]},\"employmentMembershipPeriods\":[{\"employerName\":\"Sweets R Us\",\"employerStatus\":\"C\",\"employmentStartDate\":\"1998-05-16\"}],\"benefitIllustrations\":[{\"illustrationComponents\":[{\"illustrationType\":\"ERI\",\"benefitType\":\"DC\",\"calculationMethod\":\"SMPI\",\"payableDetails\":{\"payableDate\":\"2038-09-18\",\"annualAmount\":20700,\"amountType\":\"INC\"},\"dcPot\":300000,\"survivorBenefit\":false,\"safeguardedBenefit\":false},{\"illustrationType\":\"AP\",\"benefitType\":\"DC\",\"calculationMethod\":\"SMPI\",\"payableDetails\":{\"payableDate\":\"2038-09-18\",\"annualAmount\":16215,\"amountType\":\"INC\"},\"dcPot\":235000,\"survivorBenefit\":false,\"safeguardedBenefit\":false}],\"illustrationDate\":\"2023-05-16\"}]}]\r\n}";
    }
    private string GetModifiedViewDataPayload()
    {
        return "{\r\n\t\"arrangements\": [{\"pensionProviderSchemeName\":\"ABC\",\"possibleMatchReference\":\"D9999\",\"pensionType\":\"SP\",\"pensionOrigin\":\"PC\",\"pensionStatus\":\"PC\",\"pensionStartDate\":\"2024-05-05\",\"retirementDate\":\"2042-05-05\",\"dateOfBirth\":\"2000-05-05\",\"possibleMatch\":true,\"pensionAdministrator\":{\"name\":\"ABC Your Pension\",\"contactMethods\":[{\"preferred\":false,\"contactMethodDetails\":{\"email\":\"abcmastertrust@yourpension.com\"}},{\"preferred\":true,\"contactMethodDetails\":{\"url\":\"https://www.abcyourpension.co.uk\"}},{\"preferred\":false,\"contactMethodDetails\":{\"number\":\"+44 9999999999\",\"usage\":[\"A\"]}},{\"preferred\":false,\"contactMethodDetails\":{\"postalName\":\"ABCYour Pension\",\"line1\":\"92 Victoria Lane\",\"line2\":\"Frampton Cotterell\",\"line3\":\"Bristol\",\"line4\":\"South Glocustershire\",\"postcode\":\"BS36 9DD\",\"countryCode\":\"GB\"}}]},\"employmentMembershipPeriods\":[{\"employerName\":\"ABCSweets R Us\",\"employerStatus\":\"H\",\"employmentStartDate\":\"1998-05-16\"}],\"benefitIllustrations\":[{\"illustrationComponents\":[{\"illustrationType\":\"ERI\",\"benefitType\":\"DC\",\"calculationMethod\":\"SMPI\",\"payableDetails\":{\"payableDate\":\"2038-09-18\",\"annualAmount\":20700,\"amountType\":\"INC\"},\"dcPot\":300000,\"survivorBenefit\":false,\"safeguardedBenefit\":false},{\"illustrationType\":\"AP\",\"benefitType\":\"DC\",\"calculationMethod\":\"SMPI\",\"payableDetails\":{\"payableDate\":\"2038-09-18\",\"annualAmount\":16215,\"amountType\":\"INC\"},\"dcPot\":235000,\"survivorBenefit\":false,\"safeguardedBenefit\":false}],\"illustrationDate\":\"2030-05-05\"}]}]\r\n}";

    }
    private string GetEmptyDataViewDataPayload()
    {
        return "{\r\n\t\"arrangements\": [{\"pensionProviderSchemeName\":\"\",\"possibleMatchReference\":\"\",\"pensionType\":\"DC\",\"pensionOrigin\":\"WM\",\"pensionStatus\":\"A\",\"pensionStartDate\":\"\",\"retirementDate\":\"\",\"dateOfBirth\":\"\",\"possibleMatch\":false,\"pensionAdministrator\":{\"name\":\"\",\"contactMethods\":[{\"preferred\":false,\"contactMethodDetails\":{\"email\":\"\"}},{\"preferred\":true,\"contactMethodDetails\":{\"url\":\"\"}},{\"preferred\":false,\"contactMethodDetails\":{\"number\":\"\",\"usage\":[\"M\"]}},{\"preferred\":false,\"contactMethodDetails\":{\"postalName\":\"\",\"line1\":\"92 Victoria Lane\",\"line2\":\"Frampton Cotterell\",\"line3\":\"Bristol\",\"line4\":\"South Glocustershire\",\"postcode\":\"BS36 9DD\",\"countryCode\":\"GB\"}}]},\"employmentMembershipPeriods\":[{\"employerName\":\"\",\"employerStatus\":\"C\",\"employmentStartDate\":\"1998-05-16\"}],\"benefitIllustrations\":[{\"illustrationComponents\":[{\"illustrationType\":\"ERI\",\"benefitType\":\"DC\",\"calculationMethod\":\"SMPI\",\"payableDetails\":{\"payableDate\":\"2038-09-18\",\"annualAmount\":20700,\"amountType\":\"INC\"},\"dcPot\":300000,\"survivorBenefit\":false,\"safeguardedBenefit\":false},{\"illustrationType\":\"AP\",\"benefitType\":\"DC\",\"calculationMethod\":\"SMPI\",\"payableDetails\":{\"payableDate\":\"2038-09-18\",\"annualAmount\":16215,\"amountType\":\"INC\"},\"dcPot\":235000,\"survivorBenefit\":false,\"safeguardedBenefit\":false}],\"illustrationDate\":\"\"}]}]\r\n}";
    }
    private string GetEmptyRequestPayload()
    {
        return "{\r\n\t\"pensionRetrievalRecordId\": \"\",\r\n\t\"pei\": \"\",\r\n\t\"iss\": \"\",\r\n\t\"userSessionId\": \"\",\r\n\t\"asset_guid\": \"\"\r\n}";
    }
}