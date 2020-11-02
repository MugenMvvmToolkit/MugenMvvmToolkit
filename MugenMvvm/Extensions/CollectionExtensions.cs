using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Extensions
{
    public static partial class MugenExtensions
    {
        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add<T>(this ref LazyList<T> list, T item) => list.Get().Add(item);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddIfNotNull<T>(this ref LazyList<T> list, T? item) where T : class
        {
            if (item != null)
                list.Get().Add(item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddRange<T>(this ref LazyList<T> list, IReadOnlyCollection<T>? items)
        {
            if (items != null && items.Count != 0)
                list.Get().AddRange(items);
        }

        [return: NotNullIfNotNull("collection")]
        public static IEnumerable<object?>? DecorateItems(this IObservableCollectionBase? collection)
        {
            if (collection == null)
                return null;
            var component = collection.GetComponentOptional<ICollectionDecoratorManagerComponent>();
            if (component == null)
                return collection as IEnumerable<object?> ?? collection.Cast<object?>();
            return component.DecorateItems((ICollection) collection);
        }

        public static MonitorLocker TryLock(this ICollection? collection)
        {
            if (collection == null || !collection.IsSynchronized)
                return default;
            return MonitorLocker.Lock(collection.SyncRoot);
        }

        public static void AddRange<T>(this ICollection<T> items, IEnumerable<T> value)
        {
            Should.NotBeNull(items, nameof(items));
            Should.NotBeNull(value, nameof(value));
            foreach (var item in value)
                items.Add(item);
        }

        public static T[] ToArray<T>(this T[] array)
        {
            Should.NotBeNull(array, nameof(array));
            var result = new T[array.Length];
            Array.Copy(array, 0, result, 0, array.Length);
            return result;
        }

        public static TResult[] ToArray<T, TResult>(this IReadOnlyCollection<T> collection, Func<T, TResult> selector)
        {
            Should.NotBeNull(collection, nameof(collection));
            var count = collection.Count;
            if (count == 0)
                return Default.Array<TResult>();
            var array = new TResult[count];
            count = 0;
            foreach (var item in collection)
                array[count++] = selector(item);
            return array;
        }

        public static void AddOrdered<T>(ref T[] items, T item, IComparer<T> comparer)
        {
            var array = new T[items.Length + 1];
            Array.Copy(items, 0, array, 0, items.Length);
            AddOrdered(array, item, items.Length, comparer);
            items = array;
        }

        public static void AddOrdered<T>(T[] items, T item, int size, IComparer<T> comparer)
        {
            var binarySearch = Array.BinarySearch(items, 0, size, item, comparer);
            if (binarySearch < 0)
                binarySearch = ~binarySearch;
            if (binarySearch < size)
                Array.Copy(items, binarySearch, items, binarySearch + 1, size - binarySearch);
            items[binarySearch] = item;
        }

        public static void AddOrdered<T>(List<T> list, T item, IComparer<T> comparer)
        {
            Should.NotBeNull(list, nameof(list));
            var binarySearch = list.BinarySearch(item, comparer);
            if (binarySearch < 0)
                binarySearch = ~binarySearch;
            list.Insert(binarySearch, item);
        }

        public static bool Remove<T>(ref T[] items, T item) where T : class
        {
            if (items.Length == 0)
                return false;
            if (items.Length == 1)
            {
                if (items[0] == item)
                {
                    items = Default.Array<T>();
                    return true;
                }

                return false;
            }

            T[]? array = null;
            for (var i = 0; i < items.Length; i++)
            {
                if (array == null && item == items[i])
                {
                    array = new T[items.Length - 1];
                    Array.Copy(items, 0, array, 0, i);
                    continue;
                }

                if (array != null)
                    array[i - 1] = items[i];
            }

            if (array != null)
                items = array;
            return array != null;
        }

        internal static IReadOnlyList<object>? ToReadOnlyList(this IEnumerable? enumerable)
        {
            if (enumerable == null)
                return null;
            return enumerable as IReadOnlyList<object> ?? enumerable.OfType<object>().ToList();
        }

        internal static bool Any(this IEnumerable? enumerable)
        {
            if (enumerable == null)
                return false;
            var enumerator = enumerable.GetEnumerator();
            return enumerator.MoveNext();
        }

        internal static object? FirstOrDefault(this IEnumerable? enumerable)
        {
            if (enumerable == null)
                return null;
            var enumerator = enumerable.GetEnumerator();
            enumerator.MoveNext();
            return enumerator.Current;
        }

        internal static TValue FirstOrDefault<TValue>(this HashSet<TValue> hashSet)
        {
            // ReSharper disable once GenericEnumeratorNotDisposed
            var enumerator = hashSet.GetEnumerator();
            enumerator.MoveNext();
            return enumerator.Current;
        }

        internal static KeyValuePair<TKey, TValue> FirstOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary) where TKey : notnull
        {
            // ReSharper disable once GenericEnumeratorNotDisposed
            var enumerator = dictionary.GetEnumerator();
            enumerator.MoveNext();
            return enumerator.Current;
        }

#if SPAN_API
        //https://github.com/dotnet/runtime/pull/295
        internal static SpanSplitEnumerator<char> Split(this ReadOnlySpan<char> span, char separator)
            => new SpanSplitEnumerator<char>(span, separator);
#endif

        #endregion

        #region Nested types

#if SPAN_API
        [StructLayout(LayoutKind.Auto)]
        public ref struct SpanSplitEnumerator<T> where T : IEquatable<T>
        {
            private readonly ReadOnlySpan<T> _buffer;

            private readonly ReadOnlySpan<T> _separators;
            private readonly T _separator;

            private readonly int _separatorLength;
            private readonly bool _splitOnSingleToken;

            private readonly bool _isInitialized;

            private int _startCurrent;
            private int _endCurrent;
            private int _startNext;

            public SpanSplitEnumerator<T> GetEnumerator() => this;

            public Range Current => new Range(_startCurrent, _endCurrent);

            internal SpanSplitEnumerator(ReadOnlySpan<T> span, ReadOnlySpan<T> separators)
            {
                _isInitialized = true;
                _buffer = span;
                _separators = separators;
                _separator = default!;
                _splitOnSingleToken = false;
                _separatorLength = _separators.Length != 0 ? _separators.Length : 1;
                _startCurrent = 0;
                _endCurrent = 0;
                _startNext = 0;
            }

            internal SpanSplitEnumerator(ReadOnlySpan<T> span, T separator)
            {
                _isInitialized = true;
                _buffer = span;
                _separator = separator;
                _separators = default;
                _splitOnSingleToken = true;
                _separatorLength = 1;
                _startCurrent = 0;
                _endCurrent = 0;
                _startNext = 0;
            }

            public bool MoveNext()
            {
                if (!_isInitialized || _startNext > _buffer.Length)
                    return false;

                var slice = _buffer.Slice(_startNext);
                _startCurrent = _startNext;

                var separatorIndex = _splitOnSingleToken ? slice.IndexOf(_separator) : slice.IndexOf(_separators);
                var elementLength = separatorIndex != -1 ? separatorIndex : slice.Length;

                _endCurrent = _startCurrent + elementLength;
                _startNext = _endCurrent + _separatorLength;
                return true;
            }
        }
#endif

        #endregion
    }
}