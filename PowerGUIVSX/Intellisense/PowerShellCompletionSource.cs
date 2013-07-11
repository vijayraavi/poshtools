using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;
using PowerGUIVSX;

namespace AdamDriscoll.PowerGUIVSX.Intellisense
{
    internal class PowerShellCompletionSource : ICompletionSource
    {
        private PowerShellCompletionSourceProvider m_sourceProvider;
        private ITextBuffer m_textBuffer;
        private List<Completion> m_compList;
        private VSXHost _host;


        public PowerShellCompletionSource(PowerShellCompletionSourceProvider sourceProvider, ITextBuffer textBuffer, VSXHost host)
        {
            m_sourceProvider = sourceProvider;
            m_textBuffer = textBuffer;
            _host = host;
        }

        void ICompletionSource.AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {
            List<string> strList = new List<string>();

            var text = session.TextView.TextBuffer.CurrentSnapshot.GetText();
            var currentPoint = session.TextView.Caret.Position.BufferPosition;

            //    ITextStructureNavigator navigator = m_sourceProvider.NavigatorService.GetTextStructureNavigator(m_textBuffer);
            // TextExtent extent = navigator.GetExtentOfWord(currentPoint);

            using (var ps = PowerShell.Create())
            {
                //var text = m_textBuffer.CurrentSnapshot.GetText(extent.Span);

                if (_host != null)
                {
                    ps.Runspace = _host.Runspace;
                }

                var commandCompletion = CommandCompletion.CompleteInput(text, currentPoint, new Hashtable(), ps);
                foreach (var match in commandCompletion.CompletionMatches)
                {
                    strList.Add(match.CompletionText);
                }
            }

            m_compList = new List<Completion>();
            foreach (string str in strList)
                m_compList.Add(new Completion(str, str, str, null, null));

            completionSets.Add(new CompletionSet(
                "Tokens",    //the non-localized title of the tab 
                "Tokens",    //the display title of the tab
                FindTokenSpanAtPosition(session),
                m_compList,
                null));
        }

        private ITrackingSpan FindTokenSpanAtPosition(ICompletionSession session)
        {
            SnapshotPoint currentPoint = (session.TextView.Caret.Position.BufferPosition) - 1;
            ITextStructureNavigator navigator = m_sourceProvider.NavigatorService.GetTextStructureNavigator(m_textBuffer);
            TextExtent extent = navigator.GetExtentOfWord(currentPoint);
            return currentPoint.Snapshot.CreateTrackingSpan(extent.Span, SpanTrackingMode.EdgeInclusive);
        }

        private bool m_isDisposed;
        public void Dispose()
        {
            if (!m_isDisposed)
            {
                GC.SuppressFinalize(this);
                m_isDisposed = true;
            }
        }
    }

}
