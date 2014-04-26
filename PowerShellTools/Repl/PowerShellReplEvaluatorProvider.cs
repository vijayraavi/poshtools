using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Repl;
using PowerShellTools.DebugEngine;

namespace PowerShellTools.Repl
{
    [Export(typeof(IReplEvaluatorProvider))]
    internal class PowerShellReplEvaluatorProvider : IReplEvaluatorProvider
    {
        public PowerShellReplEvaluator psEval;
        
        public IReplEvaluator GetEvaluator(string replId)
        {
            if (replId != "PowerShell") return null;

            if (psEval == null)
            {
                psEval = new PowerShellReplEvaluator(new ScriptDebugger(PowerShellToolsPackage.Instance.Host.Runspace, new ScriptBreakpoint[]{}));
            }

            return psEval;
        }
    }
}
