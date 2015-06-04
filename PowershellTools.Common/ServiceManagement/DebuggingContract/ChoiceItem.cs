using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Host;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Common.ServiceManagement.DebuggingContract
{
    [DataContract]
    public sealed class ChoiceItem
    {
        public ChoiceItem(ChoiceDescription description)
        {
            if (description == null)
            {
                throw new ArgumentNullException("description");
            }

            this.Label = description.Label;
            this.HelpMessage = description.HelpMessage;
        }

        public ChoiceItem(string label, string message)
        {
            this.Label = label;
            this.HelpMessage = message;
        }

        public string Label
        {
            get;
            private set;
        }

        public string HelpMessage
        {
            get;
            private set;
        }
    }
}
