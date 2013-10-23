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
        public const string CmdSetGuid = "099073C0-B561-4BC1-A847-92657B89A00E";
        public const uint CmdidExecuteAsScript =  0x0102;
        public const uint CmdidExecuteSelection = 0x0103;
        public const uint CmdidGotoDefinition = 0x0104;
    };
}