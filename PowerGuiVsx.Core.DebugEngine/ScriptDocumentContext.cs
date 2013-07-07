using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace PowerGuiVsx.Core.DebugEngine
{
    public class ScriptDocumentContext : IDebugDocumentContext2, IDebugCodeContext2, IEnumDebugCodeContexts2
    {
        private string _fileName;
        private string _description;
        private int _line, _column;


        public ScriptDocumentContext(string fileName, int line, int column, string description)
        {
            _fileName = fileName;
            _line = line;
            _column = column;
            _description = description;
        }

        #region Implementation of IDebugDocumentContext2

        public int GetDocument(out IDebugDocument2 ppDocument)
        {
            Trace.WriteLine("ScriptDocumentContext: GetDocument");
            ppDocument = null;
            return VSConstants.E_NOTIMPL;
        }

        public int GetName(enum_GETNAME_TYPE gnType, out string pbstrFileName)
        {
            Trace.WriteLine("ScriptDocumentContext: GetName");
            pbstrFileName = _fileName;
            return VSConstants.S_OK;
        }

        public int EnumCodeContexts(out IEnumDebugCodeContexts2 ppEnumCodeCxts)
        {
            Trace.WriteLine("ScriptDocumentContext: EnumCodeContexts");
            ppEnumCodeCxts = this;
            return VSConstants.S_OK;
        }

        public int GetLanguageInfo(ref string pbstrLanguage, ref Guid pguidLanguage)
        {
            Trace.WriteLine("ScriptDocumentContext: GetLanguageInfo");
            pguidLanguage = Guid.Empty;
            pbstrLanguage = "PowerShell";
            return VSConstants.S_OK;
        }

        public int GetStatementRange(TEXT_POSITION[] pBegPosition, TEXT_POSITION[] pEndPosition)
        {
            Trace.WriteLine("ScriptDocumentContext: GetStatementRange");

            pBegPosition[0].dwLine = (uint)_line;
            pBegPosition[0].dwColumn = (uint)_column;
            pEndPosition[0].dwLine = (uint)_line;
            pEndPosition[0].dwColumn = (uint)_column;

            return VSConstants.S_OK;
        }

        public int GetSourceRange(TEXT_POSITION[] pBegPosition, TEXT_POSITION[] pEndPosition)
        {
            Trace.WriteLine("ScriptDocumentContext: GetSourceRange");

            pBegPosition[0].dwLine = (uint)_line;
            pBegPosition[0].dwColumn = (uint)_column;
            pEndPosition[0].dwLine = (uint)_line;
            pEndPosition[0].dwColumn = (uint)_column;

            return VSConstants.S_OK;
        }

        public int Compare(enum_DOCCONTEXT_COMPARE Compare, IDebugDocumentContext2[] rgpDocContextSet, uint dwDocContextSetLen, out uint pdwDocContext)
        {
            Trace.WriteLine("ScriptDocumentContext: Compare");
            pdwDocContext = 0;
            return VSConstants.E_NOTIMPL;
        }

        public int Seek(int nCount, out IDebugDocumentContext2 ppDocContext)
        {
            Trace.WriteLine("ScriptDocumentContext: Seek");
            ppDocContext = null;
            return VSConstants.E_NOTIMPL;
        }

        #endregion


        #region Implementation of IDebugCodeContext2

        public int GetName(out string pbstrName)
        {
            Trace.WriteLine("ScriptDocumentContext: IDebugCodeContext2.GetName");
            pbstrName = "TestName";
            return VSConstants.S_OK;
        }

        public int GetInfo(enum_CONTEXT_INFO_FIELDS dwFields, CONTEXT_INFO[] pinfo)
        {
            pinfo[0].dwFields = 0;

            Trace.WriteLine("ScriptDocumentContext: IDebugCodeCotnext2.GetInfo");

            if ((dwFields & enum_CONTEXT_INFO_FIELDS.CIF_FUNCTION) != 0)
            {
                pinfo[0].bstrFunction = "TestFunc";
                pinfo[0].dwFields |= enum_CONTEXT_INFO_FIELDS.CIF_FUNCTION;    
            }

            if ((dwFields & enum_CONTEXT_INFO_FIELDS.CIF_MODULEURL) != 0)
            {
                pinfo[0].bstrModuleUrl = _fileName;
                pinfo[0].dwFields |= enum_CONTEXT_INFO_FIELDS.CIF_MODULEURL;
            }
            
            return VSConstants.S_OK;
        }

        public int Add(ulong dwCount, out IDebugMemoryContext2 ppMemCxt)
        {
            Trace.WriteLine("ScriptDocumentContext: IDebugCodeContext2.Add");
            ppMemCxt = null;
            return VSConstants.E_NOTIMPL;
        }

        public int Subtract(ulong dwCount, out IDebugMemoryContext2 ppMemCxt)
        {
            Trace.WriteLine("ScriptDocumentContext: IDebugCodeContext2.Subtract");
            ppMemCxt = null;
            return VSConstants.E_NOTIMPL;
        }

        public int Compare(enum_CONTEXT_COMPARE Compare, IDebugMemoryContext2[] rgpMemoryContextSet, uint dwMemoryContextSetLen, out uint pdwMemoryContext)
        {
            Trace.WriteLine("ScriptDocumentContext: IDebugCodeContext2.Compare");
            pdwMemoryContext = 0;
            return VSConstants.E_NOTIMPL;
        }

        public int GetDocumentContext(out IDebugDocumentContext2 ppSrcCxt)
        {
            Trace.WriteLine("ScriptDocumentContext: IDebugCodeContext2.GetDocumentContext");
            ppSrcCxt = this;
            return VSConstants.S_OK;
        }

        #endregion

        #region Implementation of IEnumDebugCodeContexts2

        List<IDebugCodeContext2> _context = new List<IDebugCodeContext2>();

        public int Next(uint celt, IDebugCodeContext2[] rgelt, ref uint pceltFetched)
        {
            if (celt == 1)
            {
                rgelt[0] = this;
                pceltFetched = 1;
            }
            return VSConstants.S_OK;
        }

        public int Skip(uint celt)
        {
            return VSConstants.S_OK;
        }

        public int Reset()
        {
            return VSConstants.S_OK;
        }

        public int Clone(out IEnumDebugCodeContexts2 ppEnum)
        {
            ppEnum = null;
            return VSConstants.E_NOTIMPL;
        }

        public int GetCount(out uint pcelt)
        {
            pcelt = 1;
            return VSConstants.S_OK;

        }

        #endregion

        public override string ToString()
        {
            return _description;
        }
    }
}
