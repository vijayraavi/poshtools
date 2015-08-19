using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PowerShellTools.Explorer
{
    internal sealed class PowerShellHost
    {
        public PowerShellHost()
        {
        }

        public Task<PSDataCollection<T>> ExecuteCommandAsync<T>(PipelineSequence sequence)
        {
            return Task.Run<PSDataCollection<T>>(() => ExecuteCommandInternal<T>(null, sequence));
        }

        public Task<PSDataCollection<T>> ExecuteCommandAsync<T>(string module, PipelineSequence sequence)
        {
            return Task.Run<PSDataCollection<T>>(() => ExecuteCommandInternal<T>(new []{module}, sequence));
        }

        public Task<PSDataCollection<T>> ExecuteCommandAsync<T>(string[] modules, PipelineSequence sequence)
        {
            return Task.Run<PSDataCollection<T>>(() => ExecuteCommandInternal<T>(modules, sequence));
        }

        private PSDataCollection<T> ExecuteCommandInternal<T>(string[] modules, PipelineSequence sequence)
        {
            PSDataCollection<T> outputCollection = new PSDataCollection<T>();

            InitialSessionState initialState = InitialSessionState.CreateDefault();
            if (modules != null)
            {
                initialState.ImportPSModule(modules);
            }

            using (PowerShell ps = PowerShell.Create(initialState))
            {
                ps.AddPipelineSequence(sequence);
                ps.Invoke<T>(null, outputCollection);
            }

            return outputCollection;
        }

        public Task<PSDataCollection<T>> ExecuteScriptAsync<T>(string[] modules, string script)
        {
            return Task.Run<PSDataCollection<T>>(() => ExecuteScriptInternal<T>(modules, script));
        }

        private PSDataCollection<T> ExecuteScriptInternal<T>(string[] modules, string script)
        {
            PSDataCollection<T> outputCollection = new PSDataCollection<T>();

            InitialSessionState initialState = InitialSessionState.CreateDefault();
            if (modules != null)
            {
                initialState.ImportPSModule(modules);
            }

            using (PowerShell ps = PowerShell.Create(initialState))
            {
                ps.AddScript(script);
                ps.Invoke<T>(null, outputCollection);
            }

            return outputCollection;
        }
    }
}
