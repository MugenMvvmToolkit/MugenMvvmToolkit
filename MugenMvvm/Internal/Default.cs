using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;
using MugenMvvm.Constants;
using MugenMvvm.ViewModels;

namespace MugenMvvm.Internal
{
    public static class Default
    {
        internal static readonly PropertyChangedEventArgs EmptyPropertyChangedArgs = new(string.Empty);
        internal static readonly PropertyChangedEventArgs CountPropertyChangedArgs = new(nameof(IList.Count));
        internal static readonly PropertyChangedEventArgs IndexerPropertyChangedArgs = new(InternalConstant.IndexerName);
        internal static readonly PropertyChangedEventArgs IsBusyPropertyChangedArgs = new(nameof(ViewModelBase.IsBusy));
        internal static readonly PropertyChangedEventArgs BusyTokenPropertyChangedArgs = new(nameof(ViewModelBase.BusyToken));
        internal static readonly NotifyCollectionChangedEventArgs ResetCollectionEventArgs = new(NotifyCollectionChangedAction.Reset);

        private static readonly int[] EmptySize = {0};
        private static readonly Dictionary<Type, Array> EmptyArrayCache = new(InternalEqualityComparer.Type);
        private static int _counter;

        public static Array Array(Type type)
        {
            Should.NotBeNull(type, nameof(type));
            if (type == typeof(object))
                return System.Array.Empty<object>();
            if (type == typeof(string))
                return System.Array.Empty<string>();
            if (type == typeof(int))
                return System.Array.Empty<int>();
            return GetEmptyArray<object>(type);
        }

        public static ReadOnlyDictionary<TKey, TValue> ReadOnlyDictionary<TKey, TValue>() where TKey : notnull => EmptyDictionaryImpl<TKey, TValue>.Instance;

        internal static int NextCounter() => Interlocked.Increment(ref _counter);

        private static Array GetEmptyArray<T>(Type type)
        {
            lock (EmptyArrayCache)
            {
                if (!EmptyArrayCache.TryGetValue(type, out var array))
                {
                    array = System.Array.CreateInstance(type, EmptySize);
                    EmptyArrayCache[type] = array;
                }

                return array;
            }
        }

        private static class EmptyDictionaryImpl<TKey, TValue> where TKey : notnull
        {
            public static readonly ReadOnlyDictionary<TKey, TValue> Instance = new(new Dictionary<TKey, TValue>());
        }
    }
}