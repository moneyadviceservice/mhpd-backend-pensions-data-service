using PDPViewDataServicedEmulator.Mocks;

namespace PDPViewDataServicedEmulator.CosmosRepository
{
    public interface IViewDataRepository
    {
        public ViewDataPayload GetViewData(string assetGuid);
    }
}