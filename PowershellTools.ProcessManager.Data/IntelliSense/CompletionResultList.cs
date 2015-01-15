using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Runtime.Serialization;
using PowershellTools.ProcessManager.Data.Common;
using System.Management.Automation;

namespace PowershellTools.ProcessManager.Data.IntelliSense
{
    /// <summary>
    /// The statement completion lists needed for IntelliSense.
    /// </summary>
    [DataContract(Namespace = Constants.ProcessManagerDataNamespace)]
    public sealed class CompletionResultList
    {
        [DataMember]
        public CompletionResult[] CompletionMatches { get; set; }

        [DataMember]
        public int ReplacementIndex { get; set; }

        [DataMember]
        public int ReplacementLength { get; set; }

        public static CompletionResultList FromCommandCompletion(CommandCompletion commandCompletion)
        {
            return new CompletionResultList()
            {
                CompletionMatches = (from match in commandCompletion.CompletionMatches
                                     select new CompletionResult(match.CompletionText, 
                                                                 match.ListItemText,
                                                                 match.ResultType,
                                                                 match.ToolTip)).ToArray(),
                ReplacementIndex = commandCompletion.ReplacementIndex,
                ReplacementLength = commandCompletion.ReplacementLength
            };
        }
    }
}
