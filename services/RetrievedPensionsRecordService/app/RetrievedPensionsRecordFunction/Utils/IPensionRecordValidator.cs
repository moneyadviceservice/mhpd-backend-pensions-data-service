using MhpdCommon.Models.MessageBodyModels;

namespace RetrievedPensionsRecordFunction.Utils;

public interface IPensionRecordValidator
{
    bool ValidateRecord(RetrievedPensionDetailsPayload? payload, out string reason);
}
