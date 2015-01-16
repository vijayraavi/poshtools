using System.ServiceModel;

namespace PowershellTools.ProcessManager.Data.IntelliSense
{
    /// <summary>
    /// Powershell service.
    /// </summary>
    [ServiceContract]
    public interface IPowershellIntelliSenseService
    {
        [OperationContract]
        CompletionResultList GetCompletionResults(string scriptUpToCaret, int carePosition);
    }
}
