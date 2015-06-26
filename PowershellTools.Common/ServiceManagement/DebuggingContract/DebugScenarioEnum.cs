using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Common.ServiceManagement.DebuggingContract
{
    /// <summary>
    /// Enumerated type representing the various debugging scenarios. See GetDebugScenario for more information
    /// </summary>
    public enum DebugScenario
    {
        Local,            // running script from Visual Studio
        RemoteSession,    // remote session debugging
        LocalAttach,      // attaching to a local process
        Unknown           // cannot determine the scenario
    }

    /// <summary>
    /// Utility methods for the DebugScenario enum
    /// </summary>
    public class DebugScenarioUtilities
    {
        /// <summary>
        /// Used by OpenFileInVS to determine correct error message to display if it is unable to open a file
        /// </summary>
        /// <param name="scenario"></param>
        /// <returns></returns>
        public static string ScenarioToFileOpenErrorMsg(DebugScenario scenario)
        {
            switch (scenario)
            {
                case DebugScenario.Local:
                    return Resources.LocalFileOpenError;
                case DebugScenario.RemoteSession:
                    return Resources.RemoteSessionFileOpenError;
                case DebugScenario.LocalAttach:
                    return Resources.LocalAttachFileOpenError;
                default:
                    return Resources.DefaultFileOpenError;
            }
        }
    }
}
