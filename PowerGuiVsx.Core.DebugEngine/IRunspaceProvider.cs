using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Runspaces;
using System.Text;

namespace PowerGuiVsx.Core.DebugEngine
{
    public interface IRunspaceRequest
    {
        void SetRunspace(Runspace runspace);
    }
}
