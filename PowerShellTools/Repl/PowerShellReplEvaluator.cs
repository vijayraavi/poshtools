
using System.Management.Automation.Runspaces;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Repl;
using Microsoft.VisualStudio.Text;
using PowerShellTools.DebugEngine;

namespace PowerShellTools.Repl
{
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
            PowerShellToolsPackage.Instance.Host.ReplWindow = window;
            return tf.StartNew(() => { Window = window; return new ExecutionResult(true); });

        }

        public void ActiveLanguageBufferChanged(ITextBuffer currentBuffer, ITextBuffer previousBuffer)
        {
            
        }

        public Task<ExecutionResult> Reset()
        {
            return tf.StartNew(() => { return new ExecutionResult(true); });
        }

        public bool CanExecuteText(string text)
        {
            return true;
        }

        public Task<ExecutionResult> ExecuteText(string text)
        {
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
    }
 
}
