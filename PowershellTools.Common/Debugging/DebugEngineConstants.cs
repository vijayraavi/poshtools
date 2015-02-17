using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Common.Debugging
{
    public static class DebugEngineConstants
    {
        /// <summary>
        /// Command format of executing powershell script file 
        /// </summary>
        /// <remarks>
        /// {0} - command
        /// {1} - arguements
        /// </remarks>
        public const string ExecutionCommandFormat = ". '{0}' {1}";

        public const string ExecutionCommandPattern = @"^\.\s\'.*?\'.*$";
        public const string ExecutionCommandFileReplacePattern = @"(?<=\.\s\').*?(?=\')";
    }
}
