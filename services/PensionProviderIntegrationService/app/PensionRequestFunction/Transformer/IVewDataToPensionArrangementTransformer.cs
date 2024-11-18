namespace PensionRequestFunction.Transformer;

public interface IVewDataToPensionArrangementTransformer
{
    string Transform(string externalAssetId, string pdpViewData, string pei, string retrievalRecordId);

    string Transform(string errorCode, string pei, string retrievalRecordId);
}
