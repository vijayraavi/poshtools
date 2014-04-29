using System;
using System.Runtime.InteropServices;
using log4net;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace PowerShellTools.Intellisense
{
    internal class PowerShellCompletionCommandHandler : IOleCommandTarget
    {
        private readonly IntelliSenseManager _intelliSenseManager;

        public IntelliSenseManager IntelliSenseManager
        {
            get { return _intelliSenseManager; }
        }


        internal PowerShellCompletionCommandHandler(IVsTextView textViewAdapter, ITextView textView,
                                                    PowerShellCompletionHandlerProvider provider)
        {
            IOleCommandTarget target;
            //add the command to the command chain
            textViewAdapter.AddCommandFilter(this, out target);
            _intelliSenseManager = new IntelliSenseManager(provider.CompletionBroker, provider.ServiceProvider, target, textView);
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            return _intelliSenseManager.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            return _intelliSenseManager.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }
    }

}