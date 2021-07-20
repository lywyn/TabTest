using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace TabTest
{
    public static class ExtensionMethods
    {
        public static void AddRange<T>(this ObservableCollection<T> lista, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                lista.Add(item);
            }
        }
    }
}
