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
        /// <remarks>
        /// Pattern sample: Maching pattern like ". 'c:\test\test.ps1' -param1 val"
        /// </remarks>
        public const string ExecutionCommandPattern = @"^\.\s\'.*?\'.*$";
        
        /// <summary>
        /// Match the script file name from execution command
        /// </summary>
        /// <remarks>
        /// Pattern sample: Matching "c:\test\test.ps1" from pattern like ". 'c:\test\test.ps1' -param val"
        /// </remarks>
        public const string ExecutionCommandFileReplacePattern = @"(?<=\.\s\').*?(?=\')";

        #region Remote file open events

        /// <summary>
        /// Powershell script to unregister the PSEdit function
        /// </summary>
        public const string UnregisterPSEditScript = @"
if ((Test-Path -Path 'function:\\global:PSEdit') -eq $true)
{
    Remove-Item -Path 'function:\\global:PSEdit' -Force
}

Get-EventSubscriber -SourceIdentifier PSISERemoteSessionOpenFile -EA Ignore | Remove-Event
";

        /// <summary>
        /// Powershell script to register any function into runspace
        /// </summary>
        public const string RegisterPSEditScript = @"
param (
    [string] $PSEditFunction
)

    Register-EngineEvent -SourceIdentifier PSISERemoteSessionOpenFile -Forward
    if ((Test-Path -Path 'function:\\global:PSEdit') -eq $false)
    {
        Set-Item -Path 'function:\\global:PSEdit' -Value $PSEditFunction
    }
";

        /// <summary>
        /// PSEdit equivalent functionality
        /// </summary>
        public const string PSEditFunctionScript = @"
param (
    [Parameter(Mandatory=$true)] [String[]] $FileNames
)

    foreach ($fileName in $FileNames)
    {
        dir $fileName | where { ! $_.PSIsContainer } | foreach {
            $filePathName = $_.FullName
            
            # Get file contents
            $contentBytes = Get-Content -Path $filePathName -Raw -Encoding Byte
            
            # Notify client for file open.
            New-Event -SourceIdentifier PSISERemoteSessionOpenFile -EventArguments @($filePathName, $contentBytes) > $null
        }
    }
";
        
        /// <summary>
        /// The parameter name of the function to be registered
        /// </summary>
        public const string RegisterPSEditParameterName = "PSEditFunction";

        #endregion


        public const string ReadHostDialogTitle = "Read-Host";

        /// <summary>
        /// The default cmdlet we used to connect PowerShell remote session
        /// </summary>
        /// <remarks>
        /// {0} - remote computer name
        /// </remarks>
        public const string EnterRemoteSessionDefaultCommand = "Enter-PSSession -ComputerName {0} -Credential ''";

        public const string ExitRemoteSessionDefaultCommand = "Exit-PSSession";

        // Powershell debugging command
        public const string Debugger_Stop = "q";
        public const string Debugger_StepOver = "v";
        public const string Debugger_StepInto = "s";
        public const string Debugger_StepOut = "o";
        public const string Debugger_Continue = "c";
    }
}
