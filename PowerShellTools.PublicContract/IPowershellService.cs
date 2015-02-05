using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.PublicContracts
{
    [Guid("cf108f4f-2e2a-44ad-907d-9a01905f7d8e")]
    public interface IPowershellService
    {
        void ExecutePowerShellCommand(string command);

        Task ExecutePowerShellCommandAsync(string command);
    }
}
