using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace SketchModeller.Utilities
{
    public static class ReadOnlyObservableCollection
    {
        public static ReadOnlyObservableCollection<T> AsReadOnlyObservable<T>(this ObservableCollection<T> source)
        {
            return new ReadOnlyObservableCollection<T>(source);
        }

        public static ReadOnlyObservableCollection<T> Empty<T>()
        {
            return EmptyHelper<T>.Instance;
        }

        private class EmptyHelper<T>
        {
            public static readonly ReadOnlyObservableCollection<T> Instance = 
                new ReadOnlyObservableCollection<T>(new ObservableCollection<T>());
        }
    }
}
