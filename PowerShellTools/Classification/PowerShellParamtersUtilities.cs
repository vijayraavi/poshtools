using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;

namespace PowerShellTools.Classification
{
    internal static class PowerShellParamtersUtilities
    {
        public static bool HasParamBlock(ITextBuffer buffer)
        {
            return true;
        }
    }
}
