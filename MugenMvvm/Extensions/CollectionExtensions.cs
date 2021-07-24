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

        public static void ApplyChangesTo<T>(this GroupHeaderChangedAction action, IList<T> items, T? item, object? args)
        {
            if (action == GroupHeaderChangedAction.Clear)
                items.Clear();
            else if (action == GroupHeaderChangedAction.ItemAdded)
                items.Add(item!);
            else if (action == GroupHeaderChangedAction.ItemRemoved)
                items.Remove(item!);
            else if (action == GroupHeaderChangedAction.ItemChanged)
                (items as IReadOnlyObservableCollection)?.RaiseItemChanged(item, args);
        }

        public static DecoratorsConfiguration Convert<T, TTo>(this DecoratorsConfiguration configuration, Func<T, TTo?, TTo?> converter, IEqualityComparer<TTo?>? comparer = null,
            Action<T, TTo>? cleanup = null) where T : notnull where TTo : class => configuration.Convert(converter, cleanup, comparer, out _);

        public static DecoratorsConfiguration Convert<T, TTo>(this DecoratorsConfiguration configuration, Func<T, TTo?, TTo?> converter, Action<T, TTo>? cleanup,
            IEqualityComparer<TTo?>? comparer, out ConvertCollectionDecorator<T, TTo> decorator) where T : notnull where TTo : class
        {
            decorator = new ConvertCollectionDecorator<T, TTo>(converter, cleanup, comparer, configuration.Priority);
            return configuration.AddDecorator(decorator);
        }

        public static DecoratorsConfiguration ConvertImmutable<T, TTo>(this DecoratorsConfiguration configuration, Func<T, TTo> converter) where TTo : class
            => configuration.ConvertImmutable(converter, out _);

        public static DecoratorsConfiguration ConvertImmutable<T, TTo>(this DecoratorsConfiguration configuration, Func<T, TTo> converter,
            out ConvertImmutableCollectionDecorator<T, TTo> decorator) where TTo : class
        {
            decorator = new ConvertImmutableCollectionDecorator<T, TTo>(converter, configuration.Priority);
            return configuration.AddDecorator(decorator);
        }

        public static DecoratorsConfiguration Filter<T>(this DecoratorsConfiguration configuration, Func<T, bool> filter, bool nullItemResult = false) =>
            configuration.Filter(filter, nullItemResult, out _);

        public static DecoratorsConfiguration Filter<T>(this DecoratorsConfiguration configuration, Func<T, bool> filter,
            bool nullItemResult, out FilterCollectionDecorator<T> decorator)
        {
            Should.NotBeNull(filter, nameof(filter));
            decorator = new FilterCollectionDecorator<T>(filter, configuration.Priority) {NullItemResult = nullItemResult};
            return configuration.AddDecorator(decorator);
        }

        public static DecoratorsConfiguration Limit<T>(this DecoratorsConfiguration configuration, int limit, Func<T, bool>? condition = null) =>
            configuration.Limit(limit, condition, out _);

        public static DecoratorsConfiguration Limit<T>(this DecoratorsConfiguration configuration, int limit, Func<T, bool>? condition,
            out LimitCollectionDecorator<T> decorator)
        {
            decorator = new LimitCollectionDecorator<T>(limit, condition, configuration.Priority);
            return configuration.AddDecorator(decorator);
        }

        public static DecoratorsConfiguration Sort<T>(this DecoratorsConfiguration configuration, IComparer<T> comparer) => configuration.Sort(comparer, out _);

        public static DecoratorsConfiguration Sort<T>(this DecoratorsConfiguration configuration, IComparer<T> comparer,
            out SortCollectionDecorator decorator)
        {
            Should.NotBeNull(comparer, nameof(comparer));
            decorator = new SortCollectionDecorator(comparer as IComparer<object?> ?? new WrapperObjectComparer<T>(comparer), configuration.Priority);
            return configuration.AddDecorator(decorator);
        }

        public static DecoratorsConfiguration Flatten<T>(this DecoratorsConfiguration configuration, Func<T, FlattenItemInfo> selector) where T : class =>
            configuration.Flatten(selector, out _);

        public static DecoratorsConfiguration Flatten<T>(this DecoratorsConfiguration configuration, Func<T, FlattenItemInfo> selector,
            out FlattenCollectionDecorator<T> decorator) where T : class
        {
            decorator = new FlattenCollectionDecorator<T>(selector, configuration.Priority);
            return configuration.AddDecorator(decorator);
        }

        public static DecoratorsConfiguration Group<T, TGroup>(this DecoratorsConfiguration configuration, Func<T, TGroup?> getGroup,
            GroupCollectionDecorator<T, TGroup>.UpdateGroupDelegate? updateGroup = null, IEqualityComparer<TGroup>? comparer = null, bool hasStableKeys = true)
            where TGroup : class => configuration.Group(getGroup, updateGroup, comparer, hasStableKeys, out _);

        public static DecoratorsConfiguration Group<T, TGroup>(this DecoratorsConfiguration configuration, Func<T, TGroup?> getGroup,
            GroupCollectionDecorator<T, TGroup>.UpdateGroupDelegate? updateGroup, IEqualityComparer<TGroup>? comparer, bool hasStableKeys,
            out GroupCollectionDecorator<T, TGroup> decorator) where TGroup : class
        {
            decorator = new GroupCollectionDecorator<T, TGroup>(getGroup, updateGroup, comparer, hasStableKeys, configuration.Priority);
            return configuration.AddDecorator(decorator);
        }

        public static DecoratorsConfiguration WithHeaderFooter(this DecoratorsConfiguration configuration, ItemOrIReadOnlyList<object> header,
            ItemOrIReadOnlyList<object> footer = default) => configuration.WithHeaderFooter(header, footer, out _);

        public static DecoratorsConfiguration WithHeaderFooter(this DecoratorsConfiguration configuration, ItemOrIReadOnlyList<object> header,
            ItemOrIReadOnlyList<object> footer, out HeaderFooterCollectionDecorator decorator)
        {
            decorator = new HeaderFooterCollectionDecorator(configuration.Priority) {Header = header, Footer = footer};
            return configuration.AddDecorator(decorator);
        }

        public static DecoratorsConfiguration PinHeaderFooter<T>(this DecoratorsConfiguration configuration, Func<T, bool?> isHeaderOrFooter,
            IComparer<T>? headerComparer = null, IComparer<T>? footerComparer = null) =>
            configuration.PinHeaderFooter(isHeaderOrFooter, headerComparer, footerComparer, out _);

        public static DecoratorsConfiguration PinHeaderFooter<T>(this DecoratorsConfiguration configuration, Func<T, bool?> isHeaderOrFooter,
            IComparer<T>? headerComparer, IComparer<T>? footerComparer, out PinHeaderFooterCollectionDecorator<T> decorator)
        {
            decorator = new PinHeaderFooterCollectionDecorator<T>(isHeaderOrFooter, headerComparer, footerComparer, configuration.Priority);
            return configuration.AddDecorator(decorator);
        }

        public static DecoratorsConfiguration AutoRefreshOnPropertyChanges<T>(this DecoratorsConfiguration configuration, ItemOrArray<string> members,
            object? args = null) where T : class => configuration.AutoRefreshOnPropertyChanges<T>(members, args, out _);

        public static DecoratorsConfiguration AutoRefreshOnPropertyChanges<T>(this DecoratorsConfiguration configuration, ItemOrArray<string> members,
            object? args, out ActionToken removeToken)
            where T : class
        {
            removeToken = configuration.Collection.GetOrAddComponent<CollectionObserver>().AutoRefreshOnPropertyChanges<T>(members, args);
            return configuration;
        }

        public static DecoratorsConfiguration AutoRefreshOnPropertyChangesDecoratedItems<T>(this DecoratorsConfiguration configuration, ItemOrArray<string> members,
            object? args = null) where T : class => configuration.AutoRefreshOnPropertyChangesDecoratedItems<T>(members, args, out _);

        public static DecoratorsConfiguration AutoRefreshOnPropertyChangesDecoratedItems<T>(this DecoratorsConfiguration configuration,
            ItemOrArray<string> members, object? args, out ActionToken removeToken) where T : class
        {
            removeToken = configuration.Collection.GetOrAddComponent<DecoratedCollectionObserver>().AutoRefreshOnPropertyChanges<T>(members, args);
            return configuration;
        }

        public static ActionToken AutoRefreshOnPropertyChanges<TType>(this CollectionObserverBase listener, ItemOrArray<string> members, object? args)
            where TType : class =>
            listener.AddItemObserver<TType, (ItemOrArray<string> members, object? args)>((s, info) =>
            {
                if (info.IsCollectionEvent)
                    return false;

                foreach (var member in s.members)
                {
                    if (info.IsMemberChanged(member))
                        return true;
                }

                return false;
            }, (s, info) => info.Collection.RaiseItemChanged(info.Item, s.args), (members, args));

        public static IReadOnlyObservableCollection AddCollectionObserver<TState>(this IReadOnlyObservableCollection collection, TState state,
            Action<TState, IReadOnlyObservableCollection> onChanged, int delay = 0) => collection.AddCollectionObserver(state, onChanged, delay, out _);

        public static IReadOnlyObservableCollection AddCollectionObserverWeak<TTarget>(this IReadOnlyObservableCollection collection, TTarget target,
            Action<TTarget, IReadOnlyObservableCollection> onChanged, int delay = 0) where TTarget : class =>
            collection.AddCollectionObserverWeak(target, onChanged, delay, out _);

        public static IReadOnlyObservableCollection AddItemObserver<T>(this IReadOnlyObservableCollection collection,
            Func<CollectionObserverBase.ChangedEventInfo<T>, bool> predicate, Action<CollectionObserverBase.ChangedEventInfo<T>> onChanged, int delay)
            where T : class =>
            collection.AddItemObserver(predicate, onChanged, delay, out _);

        public static IReadOnlyObservableCollection AddItemObserver<T, TState>(this IReadOnlyObservableCollection collection,
            Func<TState, CollectionObserverBase.ChangedEventInfo<T>, bool> predicate, Action<TState, CollectionObserverBase.ChangedEventInfo<T>> onChanged, TState state,
            int delay = 0) where T : class =>
            collection.AddItemObserver(predicate, onChanged, state, delay, out _);

        public static IReadOnlyObservableCollection AddItemObserverWeak<T, TTarget>(this IReadOnlyObservableCollection collection, TTarget target,
            Func<TTarget, CollectionObserverBase.ChangedEventInfo<T>, bool> predicate, Action<TTarget, CollectionObserverBase.ChangedEventInfo<T>> onChanged, int delay = 0)
            where T : class
            where TTarget : class =>
            collection.AddItemObserverWeak(target, predicate, onChanged, delay, out _);

        public static IReadOnlyObservableCollection AddCollectionObserver<TState>(this IReadOnlyObservableCollection collection, TState state,
            Action<TState, IReadOnlyObservableCollection> onChanged, int delay, out ActionToken removeToken)
        {
            removeToken = collection.GetOrAddComponent<CollectionObserver>().AddCollectionObserver(state, onChanged, delay);
            return collection;
        }

        public static IReadOnlyObservableCollection AddCollectionObserverWeak<TTarget>(this IReadOnlyObservableCollection collection, TTarget target,
            Action<TTarget, IReadOnlyObservableCollection> onChanged, int delay, out ActionToken removeToken) where TTarget : class
        {
            removeToken = collection.GetOrAddComponent<CollectionObserver>().AddCollectionObserverWeak(target, onChanged, delay);
            return collection;
        }

        public static IReadOnlyObservableCollection AddItemObserver<T>(this IReadOnlyObservableCollection collection,
            Func<CollectionObserverBase.ChangedEventInfo<T>, bool> predicate, Action<CollectionObserverBase.ChangedEventInfo<T>> onChanged, int delay, out ActionToken removeToken)
            where T : class
        {
            removeToken = collection.GetOrAddComponent<CollectionObserver>().AddItemObserver(predicate, onChanged, delay);
            return collection;
        }

        public static IReadOnlyObservableCollection AddItemObserver<T, TState>(this IReadOnlyObservableCollection collection,
            Func<TState, CollectionObserverBase.ChangedEventInfo<T>, bool> predicate, Action<TState, CollectionObserverBase.ChangedEventInfo<T>> onChanged, TState state, int delay,
            out ActionToken removeToken) where T : class
        {
            removeToken = collection.GetOrAddComponent<CollectionObserver>().AddItemObserver(predicate, onChanged, state, delay);
            return collection;
        }

        public static IReadOnlyObservableCollection AddItemObserverWeak<T, TTarget>(this IReadOnlyObservableCollection collection, TTarget target,
            Func<TTarget, CollectionObserverBase.ChangedEventInfo<T>, bool> predicate, Action<TTarget, CollectionObserverBase.ChangedEventInfo<T>> onChanged, int delay,
            out ActionToken removeToken)
            where T : class
            where TTarget : class
        {
            removeToken = collection.GetOrAddComponent<CollectionObserver>().AddItemObserverWeak(target, predicate, onChanged, delay);
            return collection;
        }

        public static IReadOnlyObservableCollection AddDecoratedCollectionObserver<TState>(this IReadOnlyObservableCollection collection, TState state,
            Action<TState, IReadOnlyObservableCollection> onChanged, int delay = 0) => collection.AddDecoratedCollectionObserver(state, onChanged, delay, out _);

        public static IReadOnlyObservableCollection AddDecoratedCollectionObserverWeak<TTarget>(this IReadOnlyObservableCollection collection, TTarget target,
            Action<TTarget, IReadOnlyObservableCollection> onChanged, int delay = 0) where TTarget : class =>
            collection.AddDecoratedCollectionObserverWeak(target, onChanged, delay, out _);

        public static IReadOnlyObservableCollection AddDecoratedItemObserver<T>(this IReadOnlyObservableCollection collection,
            Func<CollectionObserverBase.ChangedEventInfo<T>, bool> predicate, Action<CollectionObserverBase.ChangedEventInfo<T>> onChanged, int delay)
            where T : class =>
            collection.AddDecoratedItemObserver(predicate, onChanged, delay, out _);

        public static IReadOnlyObservableCollection AddDecoratedItemObserver<T, TState>(this IReadOnlyObservableCollection collection,
            Func<TState, CollectionObserverBase.ChangedEventInfo<T>, bool> predicate, Action<TState, CollectionObserverBase.ChangedEventInfo<T>> onChanged, TState state,
            int delay = 0) where T : class =>
            collection.AddDecoratedItemObserver(predicate, onChanged, state, delay, out _);

        public static IReadOnlyObservableCollection AddDecoratedItemObserverWeak<T, TTarget>(this IReadOnlyObservableCollection collection, TTarget target,
            Func<TTarget, CollectionObserverBase.ChangedEventInfo<T>, bool> predicate, Action<TTarget, CollectionObserverBase.ChangedEventInfo<T>> onChanged, int delay = 0)
            where T : class
            where TTarget : class =>
            collection.AddDecoratedItemObserverWeak(target, predicate, onChanged, delay, out _);

        public static IReadOnlyObservableCollection AddDecoratedCollectionObserver<TState>(this IReadOnlyObservableCollection collection, TState state,
            Action<TState, IReadOnlyObservableCollection> onChanged, int delay, out ActionToken removeToken)
        {
            removeToken = collection.GetOrAddComponent<DecoratedCollectionObserver>().AddCollectionObserver(state, onChanged, delay);
            return collection;
        }

        public static IReadOnlyObservableCollection AddDecoratedCollectionObserverWeak<TTarget>(this IReadOnlyObservableCollection collection, TTarget target,
            Action<TTarget, IReadOnlyObservableCollection> onChanged, int delay, out ActionToken removeToken) where TTarget : class
        {
            removeToken = collection.GetOrAddComponent<DecoratedCollectionObserver>().AddCollectionObserverWeak(target, onChanged, delay);
            return collection;
        }

        public static IReadOnlyObservableCollection AddDecoratedItemObserver<T>(this IReadOnlyObservableCollection collection,
            Func<CollectionObserverBase.ChangedEventInfo<T>, bool> predicate, Action<CollectionObserverBase.ChangedEventInfo<T>> onChanged, int delay, out ActionToken removeToken)
            where T : class
        {
            removeToken = collection.GetOrAddComponent<DecoratedCollectionObserver>().AddItemObserver(predicate, onChanged, delay);
            return collection;
        }

        public static IReadOnlyObservableCollection AddDecoratedItemObserver<T, TState>(this IReadOnlyObservableCollection collection,
            Func<TState, CollectionObserverBase.ChangedEventInfo<T>, bool> predicate, Action<TState, CollectionObserverBase.ChangedEventInfo<T>> onChanged, TState state, int delay,
            out ActionToken removeToken) where T : class
        {
            removeToken = collection.GetOrAddComponent<DecoratedCollectionObserver>().AddItemObserver(predicate, onChanged, state, delay);
            return collection;
        }

        public static IReadOnlyObservableCollection AddDecoratedItemObserverWeak<T, TTarget>(this IReadOnlyObservableCollection collection, TTarget target,
            Func<TTarget, CollectionObserverBase.ChangedEventInfo<T>, bool> predicate, Action<TTarget, CollectionObserverBase.ChangedEventInfo<T>> onChanged, int delay,
            out ActionToken removeToken)
            where T : class
            where TTarget : class
        {
            removeToken = collection.GetOrAddComponent<DecoratedCollectionObserver>().AddItemObserverWeak(target, predicate, onChanged, delay);
            return collection;
        }

        public static DecoratorsConfiguration ConfigureDecorators(this IReadOnlyObservableCollection collection, int? priority = null)
        {
            if (priority == null)
            {
                var array = collection.GetComponents<ICollectionDecorator>();
                priority = array.Count == 0 ? 0 : GetComponentPriority(array[array.Count - 1]);
            }

            return new DecoratorsConfiguration(collection, priority.Value);
        }

        [return: NotNullIfNotNull("collection")]
        public static IEnumerable<object?>? DecoratedItems(this IReadOnlyObservableCollection? collection)
        {
            if (collection == null)
                return null;
            var component = collection.GetComponentOptional<ICollectionDecoratorManagerComponent>();
            return component == null ? collection.AsEnumerable() : component.Decorate(collection);
        }

        public static void RaiseItemChanged(this IReadOnlyObservableCollection collection, object? item, object? args)
        {
            Should.NotBeNull(collection, nameof(collection));
            collection.GetComponentOptional<ICollectionDecoratorManagerComponent>()?.RaiseItemChanged(collection, item, args);
        }

        [MustUseReturnValue]
        public static ActionToken TryLock(this IReadOnlyObservableCollection? collection) => TryLock(target: collection);

        public static void Reset<T>(this IList<T> collection, IEnumerable<T>? value)
        {
            Should.NotBeNull(collection, nameof(collection));
            if (value == null)
            {
                collection.Clear();
                return;
            }

            if (collection is IObservableCollection<T> observableCollection)
                observableCollection.Reset(value);
            else
            {
                collection.Clear();
                collection.AddRange(value);
            }
        }

        public static ActionToken SynchronizeWith<T>(this IList<T> target, IReadOnlyObservableCollection<T> source)
        {
            Should.NotBeNull(target, nameof(target));
            Should.NotBeNull(source, nameof(source));
            using var l1 = source.TryLock();
            using var l2 = TryLock(target);
            target.Reset(source);
            return source.AddComponent(new CollectionSynchronizer<T>(target));
        }

        public static ActionToken SynchronizeDecoratedItemsWith(this IList<object?> target, IReadOnlyObservableCollection source)
        {
            Should.NotBeNull(target, nameof(target));
            Should.NotBeNull(source, nameof(source));
            using var l1 = source.TryLock();
            using var l2 = TryLock(target);
            target.Reset(source.DecoratedItems());
            return source.AddComponent(new DecoratedCollectionSynchronizer(target));
        }

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

        internal static void FindAllIndexOf(this IEnumerable<object?> items, object? item, ref ItemOrListEditor<int> indexes)
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