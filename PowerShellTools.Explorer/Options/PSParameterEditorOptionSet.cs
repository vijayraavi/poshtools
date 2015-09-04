using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Explorer
{
    internal class PSParameterEditorOptionSet : OptionSet
    {
        public PSParameterEditorOptionSet()
        {
            Options.Add(new OptionModel("HashTable", "Format as Hashtable", OptionType.Bool, "Format command as hashtable", null));
        }

        public bool FormatAsHashTable
        {
            get
            {
                var s = Options.Find(x => x.Id.Equals("HashTable", StringComparison.OrdinalIgnoreCase));
                bool v;
                return bool.TryParse(s.Value, out v) && v;
            }
        }
    }
}
