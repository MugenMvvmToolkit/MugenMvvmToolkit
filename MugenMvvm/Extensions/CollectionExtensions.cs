using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Extensions
{
    public static partial class MugenExtensions
    {
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
            if (items is List<T> list)
            {
                list.AddRange(value);
                return;
            }

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
            var index = IndexOf(items, item);
            if (index < 0)
                return false;

            RemoveAt(ref items, index);
            return true;
        }

        public static void RemoveAt<T>(ref T[] items, int index) where T : class
        {
            Should.BeValid(index < items.Length, nameof(index));
            if (items.Length == 1)
            {
                items = Default.Array<T>();
                return;
            }

            if (index == items.Length - 1)
                Array.Resize(ref items, items.Length - 1);
            else
            {
                var array = items;
                Array.Resize(ref array, array.Length - 1);
                Array.Copy(items, index + 1, array, index, items.Length - index - 1);
                items = array;
            }
        }

        public static IReadOnlyDictionary<TKey, TValue> Merge<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, IReadOnlyDictionary<TKey, TValue> values)
        {
            Should.NotBeNull(dictionary, nameof(dictionary));
            Should.NotBeNull(values, nameof(values));
            if (dictionary.Count == 0)
                return values;
            if (values.Count == 0)
                return dictionary;

#if NET461
            var result = dictionary.ToDictionary(pair => pair.Key, pair => pair.Value);
#else
            var result = new Dictionary<TKey, TValue>(dictionary);
#endif
            foreach (var value in values)
                result[value.Key] = value.Value;
            return result;
        }

        internal static int IndexOf<T>(T[] items, T item) where T : class
        {
            for (var i = 0; i < items.Length; i++)
            {
                if (items[i] == item)
                    return i;
            }

            return -1;
        }

        internal static bool RemoveRaw<T>(ref object? source, T value) where T : class
        {
            if (source == null)
                return false;
            if (source == value)
            {
                source = null;
                return true;
            }

            if (source is not T[] array)
                return false;

            if (array.Length == 1)
            {
                if (array[0] == value)
                {
                    source = null;
                    return true;
                }

                return false;
            }

            if (array.Length == 2)
            {
                if (array[0] == value)
                {
                    source = array[1];
                    return true;
                }

                if (array[1] == value)
                {
                    source = array[0];
                    return true;
                }

                return false;
            }

            if (Remove(ref array, value))
            {
                source = array;
                return true;
            }

            return false;
        }

        internal static int CountEx<T>(this IEnumerable<T>? enumerable)
        {
            if (enumerable == null)
                return 0;
            if (enumerable is IReadOnlyCollection<T> c)
                return c.Count;
            return enumerable.Count();
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

#if SPAN_API
        //https://github.com/dotnet/runtime/pull/295
        internal static SpanSplitEnumerator Split(this ReadOnlySpan<char> span, char separator) => new(span, separator);

        [StructLayout(LayoutKind.Auto)]
        public ref struct SpanSplitEnumerator
        {
            private readonly ReadOnlySpan<char> _buffer;
            private readonly char _separator;

            private int _startCurrent;
            private int _endCurrent;
            private int _startNext;

            public SpanSplitEnumerator GetEnumerator() => this;

            public Range Current => new(_startCurrent, _endCurrent);

            internal SpanSplitEnumerator(ReadOnlySpan<char> span, char separator)
            {
                _buffer = span;
                _separator = separator;
                _startCurrent = 0;
                _endCurrent = 0;
                _startNext = 0;
            }

            public bool MoveNext()
            {
                if (_startNext > _buffer.Length)
                    return false;

                var slice = _buffer.Slice(_startNext);
                _startCurrent = _startNext;

                var separatorIndex = slice.IndexOf(_separator);
                var elementLength = separatorIndex != -1 ? separatorIndex : slice.Length;

                _endCurrent = _startCurrent + elementLength;
                _startNext = _endCurrent + 1;
                return true;
            }
        }
#endif
    }
}