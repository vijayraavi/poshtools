using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Common.ServiceManagement.IntelliSenseContract
{
    /// <summary>
    /// A data contract for syntax errors.
    /// </summary>
    [DataContract]
    public sealed class ParseErrorItem
    {
        public ParseErrorItem(string message, int startOffset, int endOffset)
        {
            Message = message;
            ExtentStartOffset = startOffset;
            ExtentEndOffset = endOffset;
        }

        [DataMember]
        public string Message { get; private set; }

        [DataMember]
        public int ExtentStartOffset { get; private set; }

        [DataMember]
        public int ExtentEndOffset { get; private set; }

    }
}
