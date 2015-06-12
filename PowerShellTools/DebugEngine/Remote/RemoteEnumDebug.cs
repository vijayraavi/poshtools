using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.DebugEngine.Remote
{
    internal class RemoteEnumDebug<T> where T : class {

        private readonly T _elem;
        private bool _done;

        public RemoteEnumDebug(T elem = null)
        {
            this._elem = elem;
            Reset();
        }

        protected T Element
        {
            get { return _elem; }
        }

        public int GetCount(out uint pcelt)
        {
            pcelt = (_elem == null) ? 0u : 1u;
            return 0;
        }

        public int Next(uint celt, T[] rgelt, ref uint pceltFetched)
        {
            if (_done)
            {
                pceltFetched = 0;
                return 1;
            }
            else
            {
                pceltFetched = 1;
                rgelt[0] = _elem;
                _done = true;
                return 0;
            }
        }

        public int Reset()
        {
            _done = (_elem == null);
            return 0;
        }

        public int Skip(uint celt)
        {
            if (celt == 0)
            {
                return 0;
            }
            else if (_done)
            {
                return 1;
            }
            else
            {
                _done = true;
                return celt > 1 ? 1 : 0;
            }
        }
    }
}
