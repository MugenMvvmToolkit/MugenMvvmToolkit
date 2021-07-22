using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using MugenMvvm.Collections;
using MugenMvvm.Collections.Components;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Extensions
{
    public static partial class MugenExtensions
    {
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) where TValue : new()
        {
            Should.NotBeNull(dictionary, nameof(dictionary));
            if (dictionary.TryGetValue(key, out var v))
                return v;
            v = new TValue();
            dictionary[key] = v;
            return v;
        }

        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> getValue)
        {
            Should.NotBeNull(dictionary, nameof(dictionary));
            if (dictionary.TryGetValue(key, out var v))
                return v;
            v = getValue(key);
            dictionary[key] = v;
            return v;
        }

        public static void ApplyChangesTo<T>(this GroupHeaderChangedAction action, IList<T> items, T item, object? args)
        {
            if (action == GroupHeaderChangedAction.Clear)
                items.Clear();
            else if (action == GroupHeaderChangedAction.ItemAdded)
                items.Add(item);
            else if (action == GroupHeaderChangedAction.ItemRemoved)
                items.Remove(item);
            else if (action == GroupHeaderChangedAction.ItemChanged && item != null)
                (items as IReadOnlyObservableCollection)?.RaiseItemChanged(item, args);
        }

        public static DecoratorsConfiguration<T> ConfigureDecorators<T>(this IReadOnlyObservableCollection<T> collection, int priority = 0) => new(collection, priority);

        public static DecoratorsConfiguration<object> ConfigureDecorators(this IReadOnlyObservableCollection collection, int priority = 0) => new(collection, priority);


        public static DecoratorsConfiguration<TTo> ConvertImmutable<T, TTo>(this DecoratorsConfiguration<T> configuration, Func<T, TTo> converter) =>
            configuration.ConvertImmutable(converter, out _);

        public static DecoratorsConfiguration<TTo> ConvertImmutable<T, TTo>(this DecoratorsConfiguration<T> configuration, Func<T, TTo> converter,
            out ImmutableItemConverterCollectionDecorator<T, TTo> decorator) =>
            configuration.Cast<object>().ConvertImmutable(converter, out decorator).Cast<TTo>();

        public static DecoratorsConfiguration<object> ConvertImmutable<T, TTo>(this DecoratorsConfiguration<object> configuration, Func<T, TTo> converter) =>
            configuration.ConvertImmutable(converter, out _);

        public static DecoratorsConfiguration<object> ConvertImmutable<T, TTo>(this DecoratorsConfiguration<object> configuration, Func<T, TTo> converter,
            out ImmutableItemConverterCollectionDecorator<T, TTo> decorator)
        {
            decorator = new ImmutableItemConverterCollectionDecorator<T, TTo>(converter, configuration.Priority);
            return configuration.AddDecorator(decorator);
        }


        public static DecoratorsConfiguration<T> Filter<T>(this DecoratorsConfiguration<T> configuration, Func<T, bool> filter) => configuration.Filter(filter, out _);

        public static DecoratorsConfiguration<T> Filter<T>(this DecoratorsConfiguration<T> configuration, Func<T, bool> filter, out FilterCollectionDecorator<T> decorator) =>
            configuration.Cast<object>().Filter(filter, out decorator).Cast<T>();

        public static DecoratorsConfiguration<object> Filter<T>(this DecoratorsConfiguration<object> configuration, Func<T, bool> filter) => configuration.Filter(filter, out _);

        public static DecoratorsConfiguration<object> Filter<T>(this DecoratorsConfiguration<object> configuration, Func<T, bool> filter,
            out FilterCollectionDecorator<T> decorator)
        {
            Should.NotBeNull(filter, nameof(filter));
            decorator = new FilterCollectionDecorator<T>(filter, configuration.Priority);
            return configuration.AddDecorator(decorator);
        }


        public static DecoratorsConfiguration<T> Limit<T>(this DecoratorsConfiguration<T> configuration, int limit, Func<T, bool>? condition = null) =>
            configuration.Limit(limit, condition, out _);

        public static DecoratorsConfiguration<T> Limit<T>(this DecoratorsConfiguration<T> configuration, int limit, Func<T, bool>? condition,
            out LimitCollectionDecorator<T> decorator) =>
            configuration.Cast<object>().Limit(limit, condition, out decorator).Cast<T>();

        public static DecoratorsConfiguration<object> Limit<T>(this DecoratorsConfiguration<object> configuration, int limit, Func<T, bool>? condition = null) =>
            configuration.Limit(limit, condition, out _);

        public static DecoratorsConfiguration<object> Limit<T>(this DecoratorsConfiguration<object> configuration, int limit, Func<T, bool>? condition,
            out LimitCollectionDecorator<T> decorator)
        {
            decorator = new LimitCollectionDecorator<T>(limit, condition, configuration.Priority);
            return configuration.AddDecorator(decorator);
        }


        public static DecoratorsConfiguration<T> Sort<T>(this DecoratorsConfiguration<T> configuration, IComparer<T> comparer) => configuration.Sort(comparer, out _);

        public static DecoratorsConfiguration<T> Sort<T>(this DecoratorsConfiguration<T> configuration, IComparer<T> comparer, out SortingCollectionDecorator decorator) =>
            configuration.Cast<object>().Sort(comparer, out decorator).Cast<T>();

        public static DecoratorsConfiguration<object> Sort<T>(this DecoratorsConfiguration<object> configuration, IComparer<T> comparer) => configuration.Sort(comparer, out _);

        public static DecoratorsConfiguration<object> Sort<T>(this DecoratorsConfiguration<object> configuration, IComparer<T> comparer,
            out SortingCollectionDecorator decorator)
        {
            Should.NotBeNull(comparer, nameof(comparer));
            decorator = new SortingCollectionDecorator(comparer as IComparer<object?> ?? new WrapperObjectComparer<T>(comparer), configuration.Priority);
            return configuration.AddDecorator(decorator);
        }


        public static DecoratorsConfiguration<object> Flatten<T>(this DecoratorsConfiguration<T> configuration, Func<T, FlattenItemInfo> selector) where T : class =>
            configuration.Flatten(selector, out _);

        public static DecoratorsConfiguration<object> Flatten<T>(this DecoratorsConfiguration<T> configuration, Func<T, FlattenItemInfo> selector,
            out FlattenCollectionDecorator<T> decorator) where T : class => configuration.Cast<object>().Flatten(selector, out decorator);

        public static DecoratorsConfiguration<object> Flatten<T>(this DecoratorsConfiguration<object> configuration, Func<T, FlattenItemInfo> selector) where T : class =>
            configuration.Flatten(selector, out _);

        public static DecoratorsConfiguration<object> Flatten<T>(this DecoratorsConfiguration<object> configuration, Func<T, FlattenItemInfo> selector,
            out FlattenCollectionDecorator<T> decorator) where T : class
        {
            decorator = new FlattenCollectionDecorator<T>(selector, configuration.Priority);
            return configuration.AddDecorator(decorator);
        }


        public static DecoratorsConfiguration<object> GroupHeaderFor<T, TGroup>(this DecoratorsConfiguration<T> configuration, Func<T, TGroup?> getGroup,
            GroupHeaderCollectionDecorator<T, TGroup>.UpdateGroupDelegate? updateGroup = null, IEqualityComparer<TGroup>? comparer = null, bool hasStableKeys = true)
            where TGroup : class => configuration.GroupHeaderFor(getGroup, updateGroup, comparer, hasStableKeys, out _);

        public static DecoratorsConfiguration<object> GroupHeaderFor<T, TGroup>(this DecoratorsConfiguration<object> configuration, Func<T, TGroup?> getGroup,
            GroupHeaderCollectionDecorator<T, TGroup>.UpdateGroupDelegate? updateGroup = null, IEqualityComparer<TGroup>? comparer = null, bool hasStableKeys = true)
            where TGroup : class => configuration.GroupHeaderFor(getGroup, updateGroup, comparer, hasStableKeys, out _);

        public static DecoratorsConfiguration<object> GroupHeaderFor<T, TGroup>(this DecoratorsConfiguration<T> configuration, Func<T, TGroup?> getGroup,
            GroupHeaderCollectionDecorator<T, TGroup>.UpdateGroupDelegate? updateGroup, IEqualityComparer<TGroup>? comparer, bool hasStableKeys,
            out GroupHeaderCollectionDecorator<T, TGroup> decorator) where TGroup : class =>
            configuration.Cast<object>().GroupHeaderFor(getGroup, updateGroup, comparer, hasStableKeys, out decorator);

        public static DecoratorsConfiguration<object> GroupHeaderFor<T, TGroup>(this DecoratorsConfiguration<object> configuration, Func<T, TGroup?> getGroup,
            GroupHeaderCollectionDecorator<T, TGroup>.UpdateGroupDelegate? updateGroup, IEqualityComparer<TGroup>? comparer, bool hasStableKeys,
            out GroupHeaderCollectionDecorator<T, TGroup> decorator) where TGroup : class
        {
            decorator = new GroupHeaderCollectionDecorator<T, TGroup>(getGroup, updateGroup, comparer, hasStableKeys, configuration.Priority);
            return configuration.AddDecorator(decorator);
        }


        public static DecoratorsConfiguration<object> WithHeaderOrFooter<T>(this DecoratorsConfiguration<T> configuration, ItemOrIReadOnlyList<object> header,
            ItemOrIReadOnlyList<object> footer = default) => configuration.WithHeaderOrFooter(header, footer, out _);

        public static DecoratorsConfiguration<object> WithHeaderOrFooter<T>(this DecoratorsConfiguration<T> configuration, ItemOrIReadOnlyList<object> header,
            ItemOrIReadOnlyList<object> footer, out HeaderFooterCollectionDecorator decorator)
        {
            decorator = new HeaderFooterCollectionDecorator(configuration.Priority) {Header = header, Footer = footer};
            return configuration.AddDecorator<object>(decorator);
        }


        public static DecoratorsConfiguration<T> PinHeaderOrFooter<T>(this DecoratorsConfiguration<T> configuration, Func<T, bool?> isHeaderOrFooter,
            IComparer<T>? headerComparer = null, IComparer<T>? footerComparer = null) =>
            configuration.PinHeaderOrFooter(isHeaderOrFooter, headerComparer, footerComparer, out _);

        public static DecoratorsConfiguration<T> PinHeaderOrFooter<T>(this DecoratorsConfiguration<T> configuration, Func<T, bool?> isHeaderOrFooter,
            IComparer<T>? headerComparer, IComparer<T>? footerComparer, out ItemHeaderFooterCollectionDecorator<T> decorator) =>
            configuration.Cast<object>().PinHeaderOrFooter(isHeaderOrFooter, headerComparer, footerComparer, out decorator).Cast<T>();

        public static DecoratorsConfiguration<object> PinHeaderOrFooter<T>(this DecoratorsConfiguration<object> configuration, Func<T, bool?> isHeaderOrFooter,
            IComparer<T>? headerComparer = null, IComparer<T>? footerComparer = null) =>
            configuration.PinHeaderOrFooter(isHeaderOrFooter, headerComparer, footerComparer, out _);

        public static DecoratorsConfiguration<object> PinHeaderOrFooter<T>(this DecoratorsConfiguration<object> configuration, Func<T, bool?> isHeaderOrFooter,
            IComparer<T>? headerComparer, IComparer<T>? footerComparer, out ItemHeaderFooterCollectionDecorator<T> decorator)
        {
            decorator = new ItemHeaderFooterCollectionDecorator<T>(isHeaderOrFooter, headerComparer, footerComparer, configuration.Priority);
            return configuration.AddDecorator(decorator);
        }


        public static DecoratorsConfiguration<T> AddObserver<T, TState>(this DecoratorsConfiguration<T> configuration, TState state,
            Func<TState, ItemObserverCollectionListenerBase<T>.ChangedEventInfo, bool> canInvoke,
            Action<TState, T?> invokeAction, int delay = 0) where T : class
        {
            var component = configuration.Collection.GetOrAddComponent<ItemObserverCollectionListener<T>>();
            component.AddObserver(state, canInvoke, invokeAction, delay);
            return configuration;
        }

        public static DecoratorsConfiguration<T> AddObserverDecoratedItems<T, TState>(this DecoratorsConfiguration<T> configuration, TState state,
            Func<TState, ItemObserverCollectionListenerBase<object?>.ChangedEventInfo, bool> canInvoke,
            Action<TState, object?> invokeAction, int delay = 0)
        {
            var component = configuration.Collection.GetOrAddComponent<ItemObserverCollectionDecoratorListener>();
            component.AddObserver(state, canInvoke, invokeAction, delay);
            return configuration;
        }


        public static DecoratorsConfiguration<T> AutoRefreshOnPropertyChanges<T>(this DecoratorsConfiguration<T> configuration, ItemOrArray<string> members, object? args = null)
            where T : class => configuration.AutoRefreshOnPropertyChanges<T, T>(members, args);

        public static DecoratorsConfiguration<T> AutoRefreshOnPropertyChanges<T, TType>(this DecoratorsConfiguration<T> configuration, ItemOrArray<string> members,
            object? args = null)
            where T : class
            where TType : class
        {
            configuration.Collection.GetOrAddComponent<ItemObserverCollectionListener<T?>>()
                         .AutoRefreshOnPropertyChanges<T, TType>(configuration.Collection, members, args);
            return configuration;
        }


        public static DecoratorsConfiguration<T> AutoRefreshOnPropertyChangesDecoratedItems<T>(this DecoratorsConfiguration<T> configuration, ItemOrArray<string> members,
            object? args = null) where T : class => configuration.AutoRefreshOnPropertyChangesDecoratedItems<T, T>(members, args);

        public static DecoratorsConfiguration<T> AutoRefreshOnPropertyChangesDecoratedItems<T, TType>(this DecoratorsConfiguration<T> configuration, ItemOrArray<string> members,
            object? args = null)
            where TType : class
        {
            configuration.Collection.GetOrAddComponent<ItemObserverCollectionDecoratorListener>()
                         .AutoRefreshOnPropertyChanges<object, TType>(configuration.Collection, members, args);
            return configuration;
        }


        [return: NotNullIfNotNull("collection")]
        public static IEnumerable<object?>? DecoratedItems(this IReadOnlyObservableCollection? collection)
        {
            if (collection == null)
                return null;
            var component = collection.GetComponentOptional<ICollectionDecoratorManagerComponent>();
            return component == null ? collection.AsEnumerable() : component.Decorate(collection);
        }

        public static void RaiseItemChanged(this IReadOnlyObservableCollection collection, object item, object? args)
        {
            Should.NotBeNull(collection, nameof(collection));
            collection.GetComponentOptional<ICollectionDecoratorManagerComponent>()?.RaiseItemChanged(collection, item, args);
        }

        public static void Reset<T>(this IObservableCollection<T> collection, ItemOrArray<T> itemOrArray) => Reset(collection, (ItemOrIEnumerable<T>) itemOrArray);

        public static void Reset<T>(this IObservableCollection<T> collection, ItemOrIReadOnlyList<T> itemOrIReadOnly) => Reset(collection, (ItemOrIEnumerable<T>) itemOrIReadOnly);

        public static void Reset<T>(this IObservableCollection<T> collection, ItemOrIEnumerable<T> itemOrIEnumerable)
        {
            Should.NotBeNull(collection, nameof(collection));
            if (itemOrIEnumerable.List is IReadOnlyCollection<T> l)
                collection.Reset(l);
            else
            {
                using var _ = collection.BatchUpdate();
                collection.Clear();
                foreach (var item in itemOrIEnumerable)
                    collection.Add(item);
            }
        }

        public static ActionToken SynchronizeWith<T>(this IList<T> target, IReadOnlyObservableCollection<T> source)
        {
            Should.NotBeNull(target, nameof(target));
            Should.NotBeNull(source, nameof(source));
            using var l1 = source.TryLock();
            using var l2 = TryLock(target);
            if (target is IObservableCollection<T> observableCollection)
                observableCollection.Reset(source);
            else
            {
                target.Clear();
                target.AddRange(source);
            }

            return source.AddComponent(new CollectionSynchronizer<T>(target));
        }

        [MustUseReturnValue]
        public static ActionToken TryLock(this IReadOnlyObservableCollection? collection) => TryLock(target: collection);

        public static void AddRange<T>(this ICollection<T> items, IEnumerable<T> value)
        {
            Should.NotBeNull(items, nameof(items));
            Should.NotBeNull(value, nameof(value));
            if (items is List<T> list)
                list.AddRange(value);
            else
            {
                foreach (var item in value)
                    items.Add(item);
            }
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
                return Array.Empty<TResult>();
            var array = new TResult[count];
            count = 0;
            foreach (var item in collection)
                array[count++] = selector(item);
            return array;
        }

        public static int AddOrdered<T>(ref T[] items, T item, IComparer<T> comparer)
        {
            var array = new T[items.Length + 1];
            Array.Copy(items, 0, array, 0, items.Length);
            var index = AddOrdered(array, item, items.Length, comparer);
            items = array;
            return index;
        }

        public static int AddOrdered<T>(T[] items, T item, int size, IComparer<T> comparer)
        {
            var binarySearch = Array.BinarySearch(items, 0, size, item, comparer);
            if (binarySearch < 0)
                binarySearch = ~binarySearch;
            if (binarySearch < size)
                Array.Copy(items, binarySearch, items, binarySearch + 1, size - binarySearch);
            items[binarySearch] = item;
            return binarySearch;
        }

        public static int AddOrdered<T>(List<T> list, T item, IComparer<T> comparer)
        {
            Should.NotBeNull(list, nameof(list));
            var binarySearch = list.BinarySearch(item, comparer);
            if (binarySearch < 0)
                binarySearch = ~binarySearch;
            list.Insert(binarySearch, item);
            return binarySearch;
        }

        public static bool Remove<T>(ref T[] items, T item) where T : class
        {
            var index = Array.IndexOf(items, item);
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
                items = Array.Empty<T>();
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
            where TKey : notnull
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

#if NET461
        internal static bool Remove<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, [NotNullWhen(true)] out TValue value) where TKey : notnull
        {
            Should.NotBeNull(dictionary, nameof(dictionary));
            if (dictionary.TryGetValue(key, out value))
                return dictionary.Remove(key);
            return false;
        }
#endif

        internal static void AddRaw<T>(ref object? source, T value) where T : class
        {
            if (source == null)
                source = value;
            else if (source is T[] items)
            {
                Array.Resize(ref items, items.Length + 1);
                items[items.Length - 1] = value;
                source = items;
            }
            else
                source = new[] {(T) source, value};
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

        internal static void FindAllIndexOf(this IEnumerable<object?> items, object item, ref ItemOrListEditor<int> indexes)
        {
            var index = 0;
            foreach (var value in items)
            {
                if (Equals(item, value))
                    indexes.Add(index);
                ++index;
            }
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

        internal static IEnumerable<object?> DecoratedItems(this IEnumerable collection)
        {
            if (collection is IReadOnlyObservableCollection observable)
                return observable.DecoratedItems();
            return collection.AsEnumerable();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static IEnumerable<object?> AsEnumerable(this IEnumerable enumerable) => enumerable as IEnumerable<object?> ?? enumerable.Cast<object?>();

        internal static void Reset<T>([NotNull] ref List<T>? list, IEnumerable<T>? items)
        {
            if (list == null)
                list = items == null ? new List<T>() : new List<T>(items);
            else
            {
                list.Clear();
                if (items != null)
                    list.AddRange(items);
            }
        }

        private static void AutoRefreshOnPropertyChanges<T, TType>(this ItemObserverCollectionListenerBase<T?> listener, IReadOnlyObservableCollection collection,
            ItemOrArray<string> members, object? args = null)
            where T : class
            where TType : class =>
            listener.AddObserver((members, collection, args), (s, info) =>
            {
                if (info.Item is not TType)
                    return false;

                foreach (var member in s.members)
                {
                    if (info.IsMemberChanged(member))
                        return true;
                }

                return false;
            }, (s, item) => s.collection.RaiseItemChanged(item!, s.args));

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