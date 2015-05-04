using System;
using System.Security;
using PowerShellTools.Common;

namespace PowerShellTools.Commands.UserInterface
{
    internal static class ParameterValueComparer
    {
        public static bool AreParameterValuesEqual(object value1, object value2)
        {
            // Handle null
            if (value2 == null && value1 == null)
            {
                return true;
            }
            else if (value2 == null || value1 == null)
            {
                return false;
            }

            var ss1 = value1 as SecureString;
            var ss2 = value2 as SecureString;

            var string1 = value1 as string;
            var string2 = value2 as string;

            // Handle secure strings
            if (ss2 != null || ss1 != null)
            {
                if (ss1 == null)
                {
                    ss1 = string1.ToSecureString(); // ToSecureString handles null
                }
                else if (ss2 == null)
                {
                    ss2 = string2.ToSecureString(); // ToSecureString handles null
                }

                return ss1.ValueEqualsTo(ss2); // ValueEqualsTo handles null
            }

            // Handle strings
            if (string1 != null && string2 != null)
            {
                return string.Equals(string1, string2, StringComparison.Ordinal);
            }

            // Everything else
            return object.Equals(value2, value1);
        }
    }
}
