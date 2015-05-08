using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Common.ServiceManagement.DebuggingContract
{
    [DataContract]
    public class Variable
    {
        [DataMember]
        public string VarName { get; set; }

        [DataMember]
        public string VarValue { get; set; }

        [DataMember]
        public string Type { get; set; }

        [DataMember]
        public bool IsEnumerable { get; set; }

        [DataMember]
        public bool IsPSObject { get; set; }

        [DataMember]
        public bool IsEnum { get; set; }

        public Variable(string varName, string varVal, string type, bool isEnumerable, bool isPSObject, bool isEnum)
        {
            VarName = varName;
            VarValue = varVal;
            Type = type;
            IsEnumerable = isEnumerable;
            IsPSObject = isPSObject;
            IsEnum = isEnum;
        }

        public Variable(PSVariable var)
        {
            if (var != null)
            {
                VarName = var.Name;
                VarValue = var.Value == null ? string.Empty : var.Value.ToString();
                Type = var.Value == null ? string.Empty : var.Value.GetType().ToString();
                IsEnumerable = (var.Value is IEnumerable);
                IsPSObject = (var.Value is PSObject);
                IsEnum = (var.Value is Enum);
            }
        }

    }
}
