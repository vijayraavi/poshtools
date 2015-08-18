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
            var sourceItems = _searchTarget.SearchSourceData();
            var resultItems = new List<CommandInfo>();
            uint resultCount = 0;
            this.ErrorCode = VSConstants.S_OK;

            try
            {
                string searchString = this.SearchQuery.SearchString;
                uint progress = 0;

                foreach (CommandInfo item in sourceItems)
                {
                    if (item.Name.ToLowerInvariant().Contains(searchString.ToLowerInvariant()))
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

            base.OnStartSearch();
        }

        protected override void OnStopSearch()
        {
            this.SearchResults = 0;
        }
    }
}
