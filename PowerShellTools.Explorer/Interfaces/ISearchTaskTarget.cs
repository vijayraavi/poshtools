using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Explorer
{
    public interface ISearchTaskTarget
    {
        List<CommandInfo> SearchSourceData();
        void SearchResultData(List<CommandInfo> result);
        void ClearSearch();
    }
}
