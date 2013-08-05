using System;
using System.ComponentModel;
using System.Globalization;
using PowerGUIVsx.Project;

namespace PowerShellTools.Project
{
    public class OutputTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string)) return true;

            return base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(OutputType)) return true;

            return base.CanConvertFrom(context, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string str = value as string;

            if (str != null)
            {
                OutputType ot;
                if (Enum.TryParse(str, true, out ot))
                {
                    return ot;
                }
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(OutputType))
            {
                var name = (OutputType)value;
                return Enum.GetName(typeof (OutputType), name);
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
