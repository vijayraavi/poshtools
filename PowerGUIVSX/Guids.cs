// Guids.cs
// MUST match guids.h
using System;

namespace AdamDriscoll.PowerGUIVSX
{
    static class GuidList
    {
        public const string guidPowerGUIVSXPkgString = "58dce676-42b0-4dd6-9ee4-afbc8e582b8a";
        public const string guidPowerGUIVSXCmdSetString = "63463243-6492-4230-b29c-c0f5caaf0c5b";
        public const string guidToolWindowPersistanceString = "94791542-514e-4ceb-9897-1857a0006e38";

        public static readonly Guid guidPowerGUIVSXCmdSet = new Guid(guidPowerGUIVSXCmdSetString);
    };
}