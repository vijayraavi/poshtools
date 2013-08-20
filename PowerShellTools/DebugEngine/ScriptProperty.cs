using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace PowerShellTools.DebugEngine
{

    public class ScriptProperty : IDebugProperty2
    {
        public string Name { get; set; }

        public object Value { get; set; }

        public ScriptProperty(string name, object value)
        {
            Name = name;
            Value = value;
        }

        #region Implementation of IDebugProperty2

        public int GetPropertyInfo(enum_DEBUGPROP_INFO_FLAGS dwFields, uint dwRadix, uint dwTimeout, IDebugReference2[] rgpArgs, uint dwArgCount, DEBUG_PROPERTY_INFO[] pPropertyInfo)
        {
            if ((dwFields & enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_NAME) != 0)
            {
                pPropertyInfo[0].bstrName = Name;
                pPropertyInfo[0].dwFields = enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_NAME;
            }

            if ((dwFields & enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_VALUE) != 0)
            {
                pPropertyInfo[0].bstrValue = Value.ToString();
                pPropertyInfo[0].dwFields = enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_VALUE;
            }

            return VSConstants.S_OK;
        }

        public int SetValueAsString(string pszValue, uint dwRadix, uint dwTimeout)
        {
            throw new NotImplementedException();
        }

        public int SetValueAsReference(IDebugReference2[] rgpArgs, uint dwArgCount, IDebugReference2 pValue, uint dwTimeout)
        {
            throw new NotImplementedException();
        }

        public int EnumChildren(enum_DEBUGPROP_INFO_FLAGS dwFields, uint dwRadix, ref Guid guidFilter, enum_DBG_ATTRIB_FLAGS dwAttribFilter, string pszNameFilter, uint dwTimeout, out IEnumDebugPropertyInfo2 ppEnum)
        {
            if (Value != null)
            {
                var props = GetProperties();
                ppEnum = new ScriptPropertyCollection(props.ToArray());
                return VSConstants.S_OK;
            }

            ppEnum = null;
            return VSConstants.S_FALSE;
        }

        private IEnumerable<ScriptProperty> GetProperties()
        {
            var props = Value.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            return props.Select(propertyInfo => new ScriptProperty(propertyInfo.Name, propertyInfo.GetValue(Value, null)));
        }

        public int GetParent(out IDebugProperty2 ppParent)
        {
            throw new NotImplementedException();
        }

        public int GetDerivedMostProperty(out IDebugProperty2 ppDerivedMost)
        {
            throw new NotImplementedException();
        }

        public int GetMemoryBytes(out IDebugMemoryBytes2 ppMemoryBytes)
        {
            throw new NotImplementedException();
        }

        public int GetMemoryContext(out IDebugMemoryContext2 ppMemory)
        {
            throw new NotImplementedException();
        }

        public int GetSize(out uint pdwSize)
        {
            throw new NotImplementedException();
        }

        public int GetReference(out IDebugReference2 ppReference)
        {
            throw new NotImplementedException();
        }

        public int GetExtendedInfo(ref Guid guidExtendedInfo, out object pExtendedInfo)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class ScriptPropertyCollection : List<ScriptProperty>, IEnumDebugPropertyInfo2
    {
        private uint count;

        public ScriptPropertyCollection(ScriptDebugger debugger)
        {
            foreach (var keyVal in debugger.Variables)
            {
                var val = keyVal.Value != null ? keyVal.Value : null;
                this.Add(new ScriptProperty(keyVal.Key, val));
            }
        }

        public ScriptPropertyCollection(params ScriptProperty[] children)
        {
            foreach (var scriptProperty in children)
            {
                this.Add(scriptProperty);
            }
        }

        #region Implementation of IEnumDebugPropertyInfo2

        public int Next(uint celt, DEBUG_PROPERTY_INFO[] rgelt, out uint pceltFetched)
        {
            Trace.WriteLine("ScriptPropertyCollection : Next");
            for (var i = 0; i < celt; i++)
            {
                rgelt[i].bstrName = this[(int)(i + count)].Name;
                rgelt[i].bstrValue = this[(int)(i + count)].Value != null ? this[(int)(i + count)].Value.ToString() : "$null";
                rgelt[i].bstrType = this[(int)(i + count)].Value != null ? this[(int)(i + count)].Value.GetType().ToString() : String.Empty;
                rgelt[i].pProperty = this[(int)(i + count)];
                rgelt[i].dwAttrib = GetAttributes(this[(int) (i + count)].Value);
                rgelt[i].dwFields = enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_NAME |
                                    enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_VALUE |
                                    enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_TYPE |
                                    enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_PROP |
                                    enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_ATTRIB;
            }
            pceltFetched = celt;
            return VSConstants.S_OK;

        }

        private enum_DBG_ATTRIB_FLAGS GetAttributes(object obj)
        {
            if (obj == null)
            {
                return 0;
            }

            if (obj is string || obj is int || obj is char || obj is byte)
            {
                return 0;
            }

            return enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_OBJ_IS_EXPANDABLE;
        }

        public int Skip(uint celt)
        {
            Trace.WriteLine("ScriptPropertyCollection : Skip");
            count += celt;
            return VSConstants.S_OK;
        }

        public int Reset()
        {
            Trace.WriteLine("ScriptPropertyCollection : Reset");
            count = 0;
            return VSConstants.S_OK;
        }

        public int Clone(out IEnumDebugPropertyInfo2 ppEnum)
        {
            ppEnum = null;
            return VSConstants.E_NOTIMPL;
        }

        public int GetCount(out uint pcelt)
        {
            Trace.WriteLine("ScriptPropertyCollection : GetCount");
            pcelt = (uint)this.Count;
            return VSConstants.S_OK;
        }

        #endregion
    }
}
