using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PowerShellTools.Explorer
{
    internal static class Extensions
    {
        internal static void AddItems<T>(this ObservableCollection<T> observableCollection, IEnumerable<T> items, bool clear = false)
        {
            if (clear)
            {
                observableCollection.Clear();
            }

            foreach (T item in items) observableCollection.Add(item);
        }

        internal static void AddItems<T>(this List<T> list, IEnumerable<T> items, bool clear = false)
        {
            if (clear)
            {
                list.Clear();
            }

            list.AddRange(items);
        }

        internal static void AddItems<T>(this ObservableList<T> list, IEnumerable<T> items, bool clear = false)
        {
            if (clear)
            {
                list.Clear();
            }

            list.AddRange(items);
        }

    }
}
