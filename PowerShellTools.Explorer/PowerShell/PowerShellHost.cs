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

        //public Task<PSDataCollection<T>> ExecuteCommandAsync<T>(string command)
        //{
        //    return this.ExecuteCommandAsync<T>(command, new Dictionary<string, object>());
        //}

        //public Task<PSDataCollection<T>> ExecuteCommandAsync<T>(string[] modules, string command)
        //{
        //    return this.ExecuteCommandAsync<T>(modules, command, new Dictionary<string, object>());
        //}

        //public Task<PSDataCollection<T>> ExecuteCommandAsync<T>(string command, Dictionary<string, object> parameters)
        //{
        //    return Task.Run<PSDataCollection<T>>(() => ExecuteCommandInternal<T>(command, parameters));
        //}

        //public Task<PSDataCollection<T>> ExecuteCommandAsync<T>(string[] modules, string command, Dictionary<string, object> parameters)
        //{
        //    return Task.Run<PSDataCollection<T>>(() => ExecuteCommandInternal<T>(modules, command, parameters));
        //}

        //private PSDataCollection<T> ExecuteCommandInternal<T>(string command, Dictionary<string, object> parameters)
        //{
        //    return ExecuteCommandInternal<T>(null, command, parameters);
        //}

        //private PSDataCollection<T> ExecuteCommandInternal<T>(string[] modules, string command, Dictionary<string, object> parameters)
        //{
        //    PSDataCollection<T> outputCollection = new PSDataCollection<T>();

        //    InitialSessionState initialState = InitialSessionState.CreateDefault();
        //    if(modules != null)
        //    {
        //        initialState.ImportPSModule(modules);
        //    }

        //    using (PowerShell ps = PowerShell.Create(initialState))
        //    {
        //        ps.AddCommand(command).AddParameters(parameters);
                

        //        // begin invoke execution on the pipeline
        //        ps.Invoke<T>(null, outputCollection);
        //    }

        //    return outputCollection;
        //}

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
    }
}
