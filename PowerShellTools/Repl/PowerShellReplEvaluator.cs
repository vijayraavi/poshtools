
using System.Management.Automation.Runspaces;
using System.Threading.Tasks;
using PowerShellTools.Repl;
using Microsoft.VisualStudio.Text;
using PowerShellTools.Classification;
using PowerShellTools.DebugEngine;
using PowerShellTools.Diagnostics;

namespace PowerShellTools.Repl
{
#if POWERSHELL
    using ReplRoleAttribute = PowerShellReplRoleAttribute;
    using IReplEvaluator = IPowerShellReplEvaluator;
    using IReplWindow = IPowerShellReplWindow;
    using PowerShellTools.Common.Debugging;
#endif

    [ReplRole("Debug")]
    internal class PowerShellReplEvaluator : IReplEvaluator
    {
        public IReplWindow Window { get; set; }
        private ScriptDebugger Debugger { get; set; }
        TaskFactory<ExecutionResult> tf = new TaskFactory<ExecutionResult>();

        public PowerShellReplEvaluator(ScriptDebugger debugger)
        {
            Debugger = debugger;
        }

        public void Dispose()
        {
            
        }

        public Task<ExecutionResult> Initialize(IReplWindow window)
        {
            PowerShellToolsPackage.Debugger.ReplWindow = window;

            var page = PowerShellToolsPackage.Instance.GetDialogPage<GeneralDialogPage>();

            window.TextView.Properties.AddProperty(BufferProperties.FromRepl, null);

            window.SetOptionValue(ReplOptions.Multiline, page.MultilineRepl);
            window.SetOptionValue(ReplOptions.UseSmartUpDown, true);

            return tf.StartNew(() => { Window = window; return new ExecutionResult(true); });

        }

        public void ActiveLanguageBufferChanged(ITextBuffer currentBuffer, ITextBuffer previousBuffer)
        {
            
        }

        public Task<ExecutionResult> Reset()
        {
            return tf.StartNew(() => new ExecutionResult(true));
        }

        public bool CanExecuteText(string text)
        {
            return true;
        }

        public Task<ExecutionResult> ExecuteText(string text)
        {
            if (Debugger.DebuggingCommandReady)
                return tf.StartNew(() => { Debugger.ExecuteDebuggingCommand(text); return new ExecutionResult(true); });
            else
                return tf.StartNew(() => { Debugger.Execute(text); return new ExecutionResult(true); }); 
        }

        public void ExecuteFile(string filename)
        {
            
        }

        public string FormatClipboard()
        {
            return null;
        }

        public void AbortCommand()
        {
            Debugger.Stop();
        }

        public ExecutionResult EnterRemoteSession(string computerName)
        {
            string cmdEnterRemoteSession = string.Format(DebugEngineConstants.EnterRemoteSessionDefaultCommand, computerName);
            return ExecuteText(cmdEnterRemoteSession).Result;
        }

        public ExecutionResult ExitRemoteSession()
        {
            string cmdExitRemoteSession = string.Format(DebugEngineConstants.ExitRemoteSessionDefaultCommand);
            return ExecuteText(cmdExitRemoteSession).Result;
        }

        public bool IsRemoteSession()
        {
            return Debugger.RemoteSession;
        }
    }
 
}
