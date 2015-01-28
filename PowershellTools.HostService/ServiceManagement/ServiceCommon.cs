using Microsoft.PowerShell;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PowerShellTools.HostService.ServiceManagement
{
    public class ServiceCommon
    {
        /// <summary>
        /// TODO: Temporary logging before having logging infrastructure ready
        /// </summary>
        /// <param name="msg"></param>
        public static void Log(string msg)
        {
            Log(msg, ConsoleColor.Green);
        }

        /// <summary>
        /// TODO: Temporary logging before having logging infrastructure ready
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="c"></param>
        public static void Log(string msg, ConsoleColor c)
        {
            Console.ForegroundColor = c;
            Console.WriteLine(msg);
            Console.ResetColor();
        }
    }
}
