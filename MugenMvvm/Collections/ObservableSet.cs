using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Collections
{
    public sealed class ObservableSet<T> : ObservableSet<T, T> where T : notnull
    {
        private static readonly Func<T, T> KeyFunc = GetDefaultKey;

        public ObservableSet(IEqualityComparer<T>? comparer = null, IComponentCollectionManager? componentCollectionManager = null)
            : base(KeyFunc, comparer, componentCollectionManager)
        {
        }

        public ObservableSet(int capacity, IEqualityComparer<T>? comparer = null, IComponentCollectionManager? componentCollectionManager = null)
            : base(KeyFunc, capacity, comparer, componentCollectionManager)
        {
        }

        public ObservableSet(IEnumerable<T>? items, IEqualityComparer<T>? comparer = null, IComponentCollectionManager? componentCollectionManager = null)
            : base(KeyFunc, items, comparer, componentCollectionManager)
        {
        }

        private static T GetDefaultKey(T arg) => arg;
    }
}