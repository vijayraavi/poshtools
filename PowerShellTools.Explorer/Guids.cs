// Guids.cs
// MUST match guids.h
using System;

namespace PowerShellTools.Explorer
{
    static class GuidList
    {
        public const string guidPowerShellTools_ExplorerPkgString = "9aeeb29f-9898-4772-8158-5453c38238f8";
        public const string guidPowerShellTools_ExplorerCmdSetString = "26bdb96a-fc2a-42f3-ab08-bbc3e58c134a";
        public const string guidToolWindowPersistanceString = "dd9b7693-1385-46a9-a054-06566904f861";

        public static readonly Guid guidPowerShellTools_ExplorerCmdSet = new Guid(guidPowerShellTools_ExplorerCmdSetString);
    };
}