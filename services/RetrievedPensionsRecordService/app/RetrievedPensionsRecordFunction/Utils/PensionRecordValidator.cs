using MhpdCommon.Models.MessageBodyModels;
using MhpdCommon.Utils;

namespace RetrievedPensionsRecordFunction.Utils;

public class PensionRecordValidator(IIdValidator idValidator) : IPensionRecordValidator
{
    private readonly IIdValidator _idValidator = idValidator;

    public bool ValidateRecord(RetrievedPensionDetailsPayload? payload, out string reason)
    {
        if (payload == null)
        {
            reason = "The retrieved pension record was not a valid payload";
            return false;
        }

        if (!_idValidator.IsValidGuid(payload.PensionRetrievalRecordId))
        {
            reason = $"Missing or Invalid pensionRetrievalRecordId: {payload.PensionRetrievalRecordId}";
            return false;
        }

        if (!_idValidator.IsValidPeI(payload.Pei))
        {
            reason = $"Missing or Invalid pei: {payload.Pei}";
            return false;
        }

        reason = string.Empty;
        return true;
    }
}
