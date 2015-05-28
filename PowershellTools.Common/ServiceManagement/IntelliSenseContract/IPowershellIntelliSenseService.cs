using System.ServiceModel;

namespace PowerShellTools.Common.ServiceManagement.IntelliSenseContract
{
    /// <summary>
    /// Powershell service.
    /// </summary>
    [ServiceContract(CallbackContract = typeof(IIntelliSenseServiceCallback))]
    public interface IPowerShellIntelliSenseService
    {
        [OperationContract]
        void RequestCompletionResults(string scriptUpToCaret, int carePosition, int requestWindowId, long triggerTimeTicks);

        [OperationContract]
        void GetDummyCompletionList();

        [OperationContract]
        ParseErrorItem[] GetParseErrors(string spanText);
    }
}
