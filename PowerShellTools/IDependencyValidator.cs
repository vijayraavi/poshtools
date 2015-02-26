using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PowerShellTools
{
    public interface IDependencyValidator
    {
        bool Validate();
    }
}
