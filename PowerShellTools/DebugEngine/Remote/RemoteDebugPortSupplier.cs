using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;


namespace PowerShellTools.DebugEngine.Remote
{
    internal class RemoteDebugPortSupplier : IDebugPortSupplier2, IDebugPortSupplierDescription2
    {
        // I should figure out more about this, PTVS doesn't ref it anywhere else
        public const string PortSupplierId = "{FEB76325-D127-4E02-B59D-B16D93D46CF5}";
        public static readonly Guid PortSupplierGuid = new Guid(PortSupplierId);

        public int AddPort(IDebugPortRequest2 pRequest, out IDebugPort2 ppPort)
        {
            ppPort = null;

            string pName;
            pRequest.GetPortName(out pName);

            // this needs to be more robust
            if (!pName.StartsWith("http://"))
                pName = "http://" + pName;
            if (!pName.EndsWith(":5985/WSMAN"))
                pName += ":5985/WSMAN";

            var uri = new Uri(pName);
            ppPort = new RemoteDebugPort(this, pRequest, uri);

            return VSConstants.S_OK;
        }

        public int CanAddPort()
        {
            return VSConstants.S_OK;
        }

        public int EnumPorts(out IEnumDebugPorts2 ppEnum)
        {
            ppEnum = null;
            return VSConstants.S_OK;   
        }

        public int GetDescription(enum_PORT_SUPPLIER_DESCRIPTION_FLAGS[] pdwFlags, out string pbstrText)
        {
            pbstrText = "Allows for debugging of a PowerShell script on a remote machine. Utilizes PowerShell v5.0+ features." +
                " This version of PowerShell must be installed in order to remotely debug.";
            return VSConstants.S_OK;
        }

        public int GetPort(ref Guid guidPort, out IDebugPort2 ppPort)
        {
            ppPort = null;
            return VSConstants.E_NOTIMPL;
        }

        public int GetPortSupplierId(out Guid pguidPortSupplier)
        {
            pguidPortSupplier = PortSupplierGuid;
            return VSConstants.S_OK;
        }

        public int GetPortSupplierName(out string pbstrName)
        {
            pbstrName = "PowerShell Tools Remote Debugging";
            return VSConstants.S_OK;
        }

        public int RemovePort(IDebugPort2 pPort)
        {
            return VSConstants.E_NOTIMPL;
        }
    }
}
