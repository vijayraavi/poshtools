using System.ServiceModel;

namespace PowershellTools.Common.ServiceManagement.IntelliSenseContract
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
