using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Explorer
{
    [ComImport]
    [ComVisible(true)]
    [Guid("F0A764C0-274A-4A35-8509-D8D06C9FFD2D")]
    public interface IExceptionHandler
    {
        void HandleException(Exception exception);
    }
}
