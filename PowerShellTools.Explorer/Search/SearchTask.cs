using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace PowerShellTools.Explorer.Search
{
    public class SearchTask : VsSearchTask
    {
        private ISearchTaskTarget _searchTarget;

        public SearchTask(uint dwCookie, IVsSearchQuery pSearchQuery, IVsSearchCallback pSearchCallback, ISearchTaskTarget searchTarget)
            : base(dwCookie, pSearchQuery, pSearchCallback)
        {
            _searchTarget = searchTarget;
        }

        protected override void OnStartSearch()
        {
            // Use the original content of the text box as the target of the search. 
            var sourceItems = _searchTarget.SearchSourceData();
            var resultItems = new List<CommandInfo>();

            // Get the search option. 
            bool matchCase = false;
            // matchCase = m_toolWindow.MatchCaseOption.Value; 

            uint resultCount = 0;
            this.ErrorCode = VSConstants.S_OK;

            try
            {
                string searchString = this.SearchQuery.SearchString;

                IVsSearchToken[] tokens = new IVsSearchToken[2];

                this.SearchQuery.GetTokens(2, tokens);

                var moduleName = string.Empty;
                var commandName = string.Empty;

                var module = Regex.Match(searchString, @"module:(\w+)", RegexOptions.IgnoreCase);
                var command = Regex.Match(searchString, @"command:(\w+)", RegexOptions.IgnoreCase);

                if (module.Success)
                {
                    moduleName = module.Groups[1].ToString();
                }

                if (command.Success)
                {
                    commandName = command.Groups[1].ToString();
                }

                // Determine the results. 
                uint progress = 0;

                foreach (CommandInfo item in sourceItems)
                {
                    if (module.Success && item.ModuleName.ToLowerInvariant().Contains(moduleName.ToLowerInvariant()))
                    {
                        resultItems.Add(item);
                        resultCount++;
                    }
                    else if (command.Success && item.Name.ToLowerInvariant().Contains(commandName.ToLowerInvariant()))
                    {
                        resultItems.Add(item);
                        resultCount++;
                    }
                    else if(item.Name.ToLowerInvariant().Contains(searchString.ToLowerInvariant()))
                    {
                        resultItems.Add(item);
                        resultCount++;
                    }

                    SearchCallback.ReportProgress(this, progress++, (uint)sourceItems.Count); 
                }
            }
            catch
            {
                this.ErrorCode = VSConstants.E_FAIL;
            }
            finally
            {
                ThreadHelper.Generic.Invoke(() =>
                {
                    _searchTarget.SearchResultData(resultItems); 
                });

                this.SearchResults = resultCount;
            }

            // Call the implementation of this method in the base class. 
            // This sets the task status to complete and reports task completion. 
            base.OnStartSearch();
        }

        protected override void OnStopSearch()
        {
            this.SearchResults = 0;
        }
    }
}
