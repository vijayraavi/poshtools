using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Commands.UserInterface
{
    internal static class DataTypeConstants
    {
        // Supported Parameter types
        public const string BoolType = "Boolean";
        public const string SwitchType = "SwitchParameter";
        public const string Int32Type = "Int32";
        public const string Int64Type = "Int64";
        public const string StringType = "String";

        // TODO: Unsupported parameter types
        public const string SecureStringType = "SecureString";

        public static HashSet<string> DataTypesSet = new HashSet<string>(new[] { BoolType, SwitchType });
    }
}
