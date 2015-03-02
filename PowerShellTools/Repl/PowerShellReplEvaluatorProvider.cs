using System.ComponentModel.Composition;
using PowerShellTools.Repl;
using PowerShellTools.DebugEngine;

namespace PowerShellTools.Repl
{
#if POWERSHELL
    using IReplEvaluatorProvider = IPowerShellReplEvaluatorProvider;
    using IReplEvaluator = IPowerShellReplEvaluator;
#endif

    [Export(typeof(IReplEvaluatorProvider))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    internal class PowerShellReplEvaluatorProvider : IReplEvaluatorProvider
    {
        public PowerShellReplEvaluator psEval;

        [Import]
        internal IDependencyValidator _validator;
        
        public IReplEvaluator GetEvaluator(string replId)
        {
            if (!_validator.Validate()) return null;
            if (replId != "PowerShell") return null;

            if (psEval == null)
            {
                psEval = new PowerShellReplEvaluator(PowerShellToolsPackage.Debugger);
            }

            return psEval;
        }
    }
}
