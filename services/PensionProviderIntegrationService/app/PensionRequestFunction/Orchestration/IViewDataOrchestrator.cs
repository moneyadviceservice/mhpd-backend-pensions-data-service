namespace PensionRequestFunction.Orchestration;

public interface IViewDataOrchestrator
{
    Task<string?> GetPensionViewDataAsync(string pei, string iss, string userSessionId, string correlationId);
}
