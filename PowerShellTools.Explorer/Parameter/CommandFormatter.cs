using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PowerShellTools.Common;

namespace PowerShellTools.Explorer
{
    internal sealed class CommandFormatter
    {
        private const string DefaultHashTableName = "params";

        internal static string FormatCommandModel(CommandModel model, CommandFormatterOptions options)
        {
            var asHashTable = options.AsHashTable;
            var parameterSet = options.ParameterSet;

            StringBuilder sb = new StringBuilder();

            if (asHashTable)
            {
                sb.AppendLine(FormatParametersAsHashTable(model, parameterSet));
                sb.AppendLine(string.Format("{0} @{1}", model.Name, DefaultHashTableName));
            }
            else
            {
                sb.Append(model.Name);
                sb.Append(FormatParameters(model, parameterSet));
            }

            return sb.ToString();
        }

        private static string FormatParameters(CommandModel model, string parameterSet)
        {
            StringBuilder sb = new StringBuilder();

            foreach (ParameterModel parameter in model.Parameters)
            {
                if ((parameter.Set == parameterSet | parameter.Set == "__AllParameterSets") &&
                    !string.IsNullOrWhiteSpace(parameter.Value))
                {
                    sb.Append(FormatParameter(parameter, false));
                }
            }

            foreach (CommonParameterModel parameter in model.CommonParameters)
            {
                if (!string.IsNullOrWhiteSpace(parameter.Value))
                {
                    sb.Append(FormatParameter(parameter, false));
                }
            }

            return sb.ToString();
        }

        private static string FormatParametersAsHashTable(CommandModel model, string parameterSet)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("$params=@{");

            foreach (ParameterModel parameter in model.Parameters)
            {
                if ((parameter.Set == parameterSet | parameter.Set == "__AllParameterSets") &&
                    !string.IsNullOrWhiteSpace(parameter.Value))
                {
                    sb.AppendLine(FormatParameter(parameter, true));
                }
            }

            foreach (CommonParameterModel parameter in model.CommonParameters)
            {
                if (!string.IsNullOrWhiteSpace(parameter.Value))
                {
                    sb.AppendLine(FormatParameter(parameter, true));
                }
            }

            sb.AppendLine("}");

            return sb.ToString();
        }

        private static string FormatParameter(ParameterModel parameter, bool hash)
        {
            var type = parameter.Type;
            var name = parameter.Name;
            var value = parameter.Value;

            switch (type)
            {
                case ParameterType.Unsupported:
                case ParameterType.Array:
                case ParameterType.Float:
                case ParameterType.Double:
                case ParameterType.Decimal:
                case ParameterType.Char:
                case ParameterType.Enum:
                case ParameterType.Byte:
                case ParameterType.Int32:
                case ParameterType.Int64:
                case ParameterType.String:
                case ParameterType.Choice:
                    return FormatString(name, value, hash);
                case ParameterType.Boolean:
                    return FormatBool(name, value, hash);
                case ParameterType.Switch:
                    return FormatSwitch(name, value, hash);
                default:
                    return string.Empty;
            }
        }

        private static string FormatString(string name, string value, bool hash)
        {
            return hash ? string.Format("{0}={1};", name, QuotedString(value)) : string.Format("-{0} {1}", name, QuotedString(value));
        }

        private static string FormatBool(string name, string value, bool hash)
        {
            bool set;
            if (bool.TryParse(value, out set) && set)
            {
                return hash ? string.Format("{0}={1};", name, "$true") : string.Format("-{0} {1}", name, "$true");
            }

            return hash ? string.Format("{0}={1};", name, "$false") : string.Format("-{0} {1}", name, "$false");
        }

        private static string FormatSwitch(string name, string value, bool hash)
        {
            bool set;
            if (bool.TryParse(value, out set) && set)
            {
                return hash ? string.Format("{0}={1};", name, "$true") : string.Format("-{0}", name);
            }

            return string.Empty;
        }

        private static string QuotedString(string value)
        {
            if (value.Contains(' '))
            {
                return string.Format("\"{0}\"", value);
            }
            else
            {
                return value;
            }
        }
    }
}
