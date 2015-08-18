using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Explorer
{
    internal class ObservableList<T> : List<T>, INotifyCollectionChanged
    {
        public new void Add(T item)
        {
            base.Add(item);
            RaiseCollectionChanged(NotifyCollectionChangedAction.Add);
        }

        public new void AddRange(IEnumerable<T> items)
        {
            foreach (T item in items) { base.Add(item); }
            RaiseCollectionChanged(NotifyCollectionChangedAction.Reset);
        }

        public new void Clear()
        {
            base.Clear();
            RaiseCollectionChanged(NotifyCollectionChangedAction.Reset);
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private void RaiseCollectionChanged(NotifyCollectionChangedAction a)
        {
            NotifyCollectionChangedEventHandler h = CollectionChanged;
            if (h != null)
            {
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(a));
            }
        }
    }
}
