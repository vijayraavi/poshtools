using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Explorer
{
    internal static class PowerShellHelper
    {
        public static string GetCommandInfoHelpUrl(CommandInfo info)
        {
            try
            {
                // Some commands throw a 'new not supported' exception
                // when trying to get the metadata from the command
                var meta = new CommandMetadata(info);
                return meta.HelpUri;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
