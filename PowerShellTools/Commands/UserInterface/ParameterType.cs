using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Commands.UserInterface
{
    internal enum ParameterType
    {
        Unknown,
        Boolean,
        Switch,
        Integer,
        String,
        SecureString,
    }
}
