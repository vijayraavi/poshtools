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


        /// <summary>
        /// Match if that is an execution command
        /// </summary>
        public const string ExecutionCommandPattern = @"^\.\s\'.*?\'.*$";
        
        /// <summary>
        /// Match the scrip file name from execution command
        /// </summary>
        public const string ExecutionCommandFileReplacePattern = @"(?<=\.\s\').*?(?=\')";

        public const string ReadHostDialogTitle = "Read-Host";
    }
}
