using System.ServiceModel;

namespace PowerShellTools.Common.ServiceManagement.IntelliSenseContract
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
