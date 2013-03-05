using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace PowerGUIVsx.Project.Utils
{
    public class FileListTypeConverter : StringConverter
    {
        private string[] _Stuff = new string[] { "Value 1", "Value 2", "Value @!$__3 With We!rd chars" };

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(_Stuff);
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }
    }
}
