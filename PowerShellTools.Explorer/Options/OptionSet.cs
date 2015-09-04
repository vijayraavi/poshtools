using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Explorer
{
    internal abstract class OptionSet
    {
        private List<OptionModel> _options = new List<OptionModel>();

        public OptionSet()
        {
        }

        public List<OptionModel> Options
        {
            get
            {
                return _options;
            }
        }
    }
}
