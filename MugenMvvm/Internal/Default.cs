using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Constants;
using MugenMvvm.ViewModels;

namespace MugenMvvm.Internal
{
    public static class Default
    {
        public static readonly Task CompletedTask = Task.CompletedTask;

        internal static readonly PropertyChangedEventArgs EmptyPropertyChangedArgs = new(string.Empty);
        internal static readonly PropertyChangedEventArgs CountPropertyChangedArgs = new(nameof(IList.Count));
        internal static readonly PropertyChangedEventArgs IndexerPropertyChangedArgs = new(InternalConstant.IndexerName);
        internal static readonly PropertyChangedEventArgs IsBusyPropertyChangedArgs = new(nameof(ViewModelBase.IsBusy));
        internal static readonly PropertyChangedEventArgs BusyTokenPropertyChangedArgs = new(nameof(ViewModelBase.BusyToken));
        internal static readonly NotifyCollectionChangedEventArgs ResetCollectionEventArgs = new(NotifyCollectionChangedAction.Reset);

        private static readonly int[] EmptySize = {0};
        private static readonly Dictionary<Type, Array> EmptyArrayCache = new(InternalEqualityComparer.Type);
        private static int _counter;

        public static T[] Array<T>() => EmptyArrayImpl<T>.Instance;

        public static Array Array(Type type)
        {
            Should.NotBeNull(type, nameof(type));
            if (type == typeof(object))
                return EmptyArrayImpl<object>.Instance;
            if (type == typeof(string))
                return EmptyArrayImpl<string>.Instance;
            if (type == typeof(int))
                return EmptyArrayImpl<int>.Instance;
            return GetEmptyArray<object>(type, false);
        }

        public static ReadOnlyDictionary<TKey, TValue> ReadOnlyDictionary<TKey, TValue>() where TKey : notnull => EmptyDictionaryImpl<TKey, TValue>.Instance;

        internal static int NextCounter() => Interlocked.Increment(ref _counter);

        private static Array GetEmptyArray<T>(Type type, bool isGeneric)
        {
            if (isGeneric)
                type = typeof(T);
            lock (EmptyArrayCache)
            {
                if (!EmptyArrayCache.TryGetValue(type, out var array))
                {
                    array = isGeneric ? new T[0] : System.Array.CreateInstance(type, EmptySize);
                    EmptyArrayCache[type] = array;
                }

                return array;
            }
        }

        private static class EmptyArrayImpl<T>
        {
            public static readonly T[] Instance = (T[]) GetEmptyArray<T>(typeof(Default), true);
        }

        private static class EmptyDictionaryImpl<TKey, TValue> where TKey : notnull
        {
            public static readonly ReadOnlyDictionary<TKey, TValue> Instance = new(new Dictionary<TKey, TValue>());
        }
    }
}