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
        private Runspace _runspace;

        public Runspace Runspace
        {
            get
            {
                return _runspace;
            }
        }

        public ServiceCommon(PSHost psHost)
        {
            InitializeRunspace(psHost);
        }

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

        #region private helpers

        private void InitializeRunspace(PSHost psHost)
        {
            Log("Initializing run space with debugger", ConsoleColor.Green);
            InitialSessionState iss = InitialSessionState.CreateDefault();
            iss.ApartmentState = ApartmentState.STA;
            iss.ThreadOptions = PSThreadOptions.ReuseThread;

            _runspace = RunspaceFactory.CreateRunspace(psHost, iss);
            _runspace.Open();

            ImportPoshToolsModule();
            LoadProfile();

            SetupExecutionPolicy();
        }

        private void ImportPoshToolsModule()
        {
            using (PowerShell ps = PowerShell.Create())
            {
                try
                {
                    var assemblyLocation = Assembly.GetExecutingAssembly().Location;
                    ps.Runspace = _runspace;
                    ps.AddScript("Import-Module '" + assemblyLocation + "'");
                    ps.Invoke();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to load profile.", ex);
                }
            }
        }

        private void LoadProfile()
        {
            using (PowerShell ps = PowerShell.Create())
            {
                try
                {
                    var myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    var windowsPowerShell = Path.Combine(myDocuments, "WindowsPowerShell");
                    var profile = Path.Combine(windowsPowerShell, "PoshTools_profile.ps1");

                    var fi = new FileInfo(profile);
                    if (!fi.Exists)
                    {
                        return;
                    }

                    ps.Runspace = _runspace;
                    ps.AddScript(". '" + profile + "'");
                    ps.Invoke();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to load profile.", ex);
                }
            }
        }

        private void SetupExecutionPolicy()
        {
            SetExecutionPolicy(ExecutionPolicy.RemoteSigned, ExecutionPolicyScope.Process);
        }

        private void SetExecutionPolicy(ExecutionPolicy policy, ExecutionPolicyScope scope)
        {
            using (PowerShell ps = PowerShell.Create())
            {
                ps.Runspace = _runspace;
                ps.AddCommand("Set-ExecutionPolicy")
                    .AddParameter("ExecutionPolicy", policy)
                    .AddParameter("Scope", scope);
                ps.Invoke();
            }
        }

        #endregion
    }
}
