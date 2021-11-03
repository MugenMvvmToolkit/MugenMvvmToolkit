using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.ViewModels;

namespace MugenMvvm.Internal
{
    public static class Default
    {
        public static readonly Task<bool> TrueTask = Task.FromResult(true);
        public static readonly Task<bool> FalseTask = Task.FromResult(false);

        internal static readonly PropertyChangedEventArgs EmptyPropertyChangedArgs = new(string.Empty);
        internal static readonly PropertyChangedEventArgs CountPropertyChangedArgs = new(nameof(IList.Count));
        internal static readonly PropertyChangedEventArgs IndexerPropertyChangedArgs = new(InternalConstant.IndexerName);
        internal static readonly PropertyChangedEventArgs IsBusyPropertyChangedArgs = new(nameof(ViewModelBase.IsBusy));
        internal static readonly PropertyChangedEventArgs BusyTokenPropertyChangedArgs = new(nameof(ViewModelBase.BusyToken));
        internal static readonly PropertyChangedEventArgs SelectedItemChangedArgs = new(nameof(ISelectedItemTracker<object>.SelectedItem));
        internal static readonly NotifyCollectionChangedEventArgs ResetCollectionEventArgs = new(NotifyCollectionChangedAction.Reset);

        private static readonly int[] EmptySize = {0};
        private static readonly Dictionary<Type, Array> EmptyArrayCache = new(InternalEqualityComparer.Type);
        private static int _counter;

        public static IEqualityComparer<object?> ReferenceEqualityComparer => InternalEqualityComparer.Reference;

        public static Array Array(Type type)
        {
            Should.NotBeNull(type, nameof(type));
            if (type == typeof(object))
                return System.Array.Empty<object>();
            if (type == typeof(string))
                return System.Array.Empty<string>();
            if (type == typeof(int))
                return System.Array.Empty<int>();
            return GetEmptyArray(type);
        }

        public static ReadOnlyDictionary<TKey, TValue> ReadOnlyDictionary<TKey, TValue>() where TKey : notnull => EmptyDictionaryImpl<TKey, TValue>.Instance;

        public static IEnumerable<T> Enumerable<T>() => EmptyEnumerableImpl<T>.Instance;

        public static IReadOnlyCollection<T> ReadOnlyCollection<T>() => EmptyEnumerableImpl<T>.Instance;

        public static IReadOnlyList<T> ReadOnlyList<T>() => EmptyEnumerableImpl<T>.Instance;

        public static IEnumerator<T> Enumerator<T>() => EmptyEnumerableImpl<T>.Instance;

        public static IEnumerable<T> SingleItemEnumerable<T>(T item) => new SingleItemEnumerableImpl<T>(item);

        public static IEnumerator<T> SingleItemEnumerator<T>(T item) => new SingleItemEnumerableImpl<T>(item);

        internal static int NextCounter() => Interlocked.Increment(ref _counter);

        private static Array GetEmptyArray(Type type)
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

        private sealed class SingleItemEnumerableImpl<T> : IReadOnlyList<T>, IEnumerator<T>
        {
            private bool _moved;

            public SingleItemEnumerableImpl(T item)
            {
                Current = item;
            }

            public T Current { get; }

            public int Count => 1;

            object? IEnumerator.Current => Current;

            public T this[int index]
            {
                get
                {
                    if (index != 0)
                        ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(index));
                    return Current;
                }
            }

            public void Dispose()
            {
            }

            public IEnumerator<T> GetEnumerator() => new SingleItemEnumerableImpl<T>(Current);

            public bool MoveNext()
            {
                if (_moved)
                    return false;
                _moved = true;
                return true;
            }

            public void Reset() => _moved = false;

            IEnumerator IEnumerable.GetEnumerator() => this;
        }

        private sealed class EmptyEnumerableImpl<T> : IReadOnlyList<T>, IEnumerator<T>
        {
            public static readonly EmptyEnumerableImpl<T> Instance = new();

            private EmptyEnumerableImpl()
            {
            }

            public T Current => default!;

            public int Count => 0;

            object? IEnumerator.Current => null;

            public T this[int index]
            {
                get
                {
                    ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(index));
                    return default!;
                }
            }

            public void Dispose()
            {
            }

            public IEnumerator<T> GetEnumerator() => this;

            public bool MoveNext() => false;

            public void Reset()
            {
            }

            IEnumerator IEnumerable.GetEnumerator() => this;
        }
    }
}