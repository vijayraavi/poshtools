// Guids.cs
// MUST match guids.h

using System;

namespace PowerShellTools
{
    static class GuidList
    {
        public const string PowerShellToolsPackageGuid = "58dce676-42b0-4dd6-9ee4-afbc8e582b8a";
        public const string PowerShellToolsProjectPackageGuid = "2F99237E-E34F-4A3D-A337-500E4B3167B8";
        public const string PowerShellGeneralPropertiesPageGuid = "C9619BDD-D1B3-4ACA-ADF3-2323EB62315E";
        public const string PowerShellModulePropertiesPageGuid = "C9619BDD-D1B3-4ACA-ADF3-2323EB623154";


        public const string guidPowerGUIVSXCmdSetString = "63463243-6492-4230-b29c-c0f5caaf0c5b";
        public const string guidToolWindowPersistanceString = "94791542-514e-4ceb-9897-1857a0006e38";

        public static readonly Guid guidPowerGUIVSXCmdSet = new Guid(guidPowerGUIVSXCmdSetString);
    };
}