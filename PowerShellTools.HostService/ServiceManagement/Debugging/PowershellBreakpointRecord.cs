using PowerShellTools.Common.ServiceManagement.DebuggingContract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PowerShellTools.HostService.ServiceManagement.Debugging
{
    internal sealed class PowershellBreakpointRecord
    {
        private PowershellBreakpoint _psBreakpoint;
        private int _id;

        public PowershellBreakpointRecord(PowershellBreakpoint bp, int id)
        {
            _psBreakpoint = bp;
            _id = id;
        }

        public PowershellBreakpoint PSBreakpoint
        {
            get
            {
                return _psBreakpoint;
            }
        }

        public int Id
        {
            get
            {
                return _id;
            }
        }
    }
}
