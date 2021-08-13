using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MugenMvvm.Collections;
using MugenMvvm.Collections.Components;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Extensions
{
    public static partial class MugenExtensions
    {
        public static DecoratorsConfiguration Bind(this IReadOnlyObservableCollection collection, bool disposeSourceOnDispose = false) =>
            collection.Bind<object?>(out _, disposeSourceOnDispose);

        public static DecoratorsConfiguration Bind<T>(this IReadOnlyObservableCollection collection, bool disposeSourceOnDispose = false) =>
            collection.Bind<T>(out _, disposeSourceOnDispose);

        public static DecoratorsConfiguration Bind(this IReadOnlyObservableCollection collection, out IReadOnlyObservableCollection<object?> result,
            bool disposeSourceOnDispose = false) => collection.Bind<object>(out result, disposeSourceOnDispose);

        public static DecoratorsConfiguration Bind<T>(this IReadOnlyObservableCollection collection, out IReadOnlyObservableCollection<T> result,
            bool disposeSourceOnDispose = false)
        {
            collection.ConfigureDecorators().Bind(out result, disposeSourceOnDispose);
            return result.ConfigureDecorators(0);
        }

        public static DecoratorsConfiguration ConfigureDecorators(this IReadOnlyObservableCollection collection, int? priority = null, int step = 10)
        {
            Should.NotBeNull(collection, nameof(collection));
            if (priority == null)
            {
                var array = collection.GetComponents<ICollectionDecorator>();
                priority = array.Count == 0 ? 0 : GetComponentPriority(array[array.Count - 1]) - step;
            }

            return new DecoratorsConfiguration(collection, priority.Value, step);
        }

        [return: NotNullIfNotNull("collection")]
        public static IEnumerable<object?>? DecoratedItems(this IReadOnlyObservableCollection? collection)
        {
            if (collection == null)
                return null;
            var component = collection.GetComponentOptional<ICollectionDecoratorManagerComponent>();
            return component == null ? collection.AsEnumerable() : component.Decorate(collection);
        }

        public static IEnumerable<T?> DecoratedItems<T>(this IReadOnlyObservableCollection? collection, ICollectionDecorator decorator)
        {
            Should.NotBeNull(decorator, nameof(decorator));
            var decoratorManager = collection?.GetComponentOptional<ICollectionDecoratorManagerComponent>();
            if (decoratorManager == null)
                yield break;

            using var l = collection!.Lock();
            foreach (T? o in decoratorManager.Decorate(collection, decorator))
                yield return o;
        }

        public static ICollectionBatchUpdateManagerComponent GetBatchUpdateManager(this IReadOnlyObservableCollection collection) =>
            collection.GetOrAddComponent<ICollectionBatchUpdateManagerComponent, object?>(null, (_, _) => new CollectionBatchUpdateManager());

        public static bool IsInBatch(this IReadOnlyObservableCollection collection, BatchUpdateType? batchUpdateType)
        {
            Should.NotBeNull(collection, nameof(collection));
            return collection.GetBatchUpdateManager().IsInBatch(collection, batchUpdateType ?? BatchUpdateType.Source);
        }

        public static ActionToken BatchUpdate(this IReadOnlyObservableCollection collection, BatchUpdateType? batchUpdateType = null)
        {
            Should.NotBeNull(collection, nameof(collection));
            batchUpdateType ??= BatchUpdateType.Source;
            collection.GetBatchUpdateManager().BeginBatchUpdate(collection, batchUpdateType);
            return ActionToken.FromDelegate((c, t) =>
            {
                var observableCollection = (IReadOnlyObservableCollection)c!;
                observableCollection.GetBatchUpdateManager().EndBatchUpdate(observableCollection, (BatchUpdateType)t!);
            }, collection, batchUpdateType);
        }

        public static void RaiseItemChanged(this IReadOnlyObservableCollection collection, object? item, object? args = null)
        {
            Should.NotBeNull(collection, nameof(collection));
            collection.GetComponentOptional<ICollectionDecoratorManagerComponent>()?.RaiseItemChanged(collection, item, args);
        }

        public static IReadOnlyObservableCollection Subscribe<T>(this IReadOnlyObservableCollection collection,
            Func<CollectionChangedEventInfo<T>, bool> predicate, Action<ItemOrArray<CollectionChangedEventInfo<T>>> onChanged, int delay = 0,
            bool listenItemChanges = true)
            where T : class =>
            collection.Subscribe(predicate, onChanged, delay, listenItemChanges, out _);

        public static IReadOnlyObservableCollection Subscribe<T, TState>(this IReadOnlyObservableCollection collection,
            Func<TState, CollectionChangedEventInfo<T>, bool> predicate, Action<TState, ItemOrArray<CollectionChangedEventInfo<T>>> onChanged,
            TState state, int delay = 0, bool listenItemChanges = true) where T : class =>
            collection.Subscribe(predicate, onChanged, state, delay, listenItemChanges, out _);

        public static IReadOnlyObservableCollection SubscribeWeak<T, TTarget>(this IReadOnlyObservableCollection collection, TTarget target,
            Func<TTarget, CollectionChangedEventInfo<T>, bool> predicate, Action<TTarget, ItemOrArray<CollectionChangedEventInfo<T>>> onChanged,
            int delay = 0, bool listenItemChanges = true)
            where T : class
            where TTarget : class =>
            collection.SubscribeWeak(target, predicate, onChanged, delay, listenItemChanges, out _);

        public static IReadOnlyObservableCollection Subscribe<T>(this IReadOnlyObservableCollection collection,
            Func<CollectionChangedEventInfo<T>, bool> predicate, Action<ItemOrArray<CollectionChangedEventInfo<T>>> onChanged, int delay,
            bool listenItemChanges, out ActionToken removeToken)
            where T : class
        {
            removeToken = collection.GetOrAddComponent<CollectionObserver>().AddObserver(predicate, onChanged, delay, listenItemChanges);
            return collection;
        }

        public static IReadOnlyObservableCollection Subscribe<T, TState>(this IReadOnlyObservableCollection collection,
            Func<TState, CollectionChangedEventInfo<T>, bool> predicate, Action<TState, ItemOrArray<CollectionChangedEventInfo<T>>> onChanged,
            TState state, int delay, bool listenItemChanges, out ActionToken removeToken) where T : class
        {
            removeToken = collection.GetOrAddComponent<CollectionObserver>().AddObserver(predicate, onChanged, state, delay, listenItemChanges);
            return collection;
        }

        public static IReadOnlyObservableCollection SubscribeWeak<T, TTarget>(this IReadOnlyObservableCollection collection, TTarget target,
            Func<TTarget, CollectionChangedEventInfo<T>, bool> predicate, Action<TTarget, ItemOrArray<CollectionChangedEventInfo<T>>> onChanged,
            int delay, bool listenItemChanges, out ActionToken removeToken)
            where T : class
            where TTarget : class
        {
            removeToken = collection.GetOrAddComponent<CollectionObserver>().AddObserverWeak(target, predicate, onChanged, delay, listenItemChanges);
            return collection;
        }

        public static IReadOnlyObservableCollection<T> Subscribe<T>(this IReadOnlyObservableCollection<T> collection,
            Func<CollectionChangedEventInfo<T>, bool> predicate, Action<ItemOrArray<CollectionChangedEventInfo<T>>> onChanged, int delay = 0,
            bool listenItemChanges = true)
            where T : class =>
            collection.Subscribe(predicate, onChanged, delay, listenItemChanges, out _);

        public static IReadOnlyObservableCollection<T> Subscribe<T, TState>(this IReadOnlyObservableCollection<T> collection, TState state,
            Func<TState, CollectionChangedEventInfo<T>, bool> predicate, Action<TState, ItemOrArray<CollectionChangedEventInfo<T>>> onChanged,
            int delay = 0, bool listenItemChanges = true)
            where T : class =>
            collection.Subscribe(state, predicate, onChanged, delay, listenItemChanges, out _);

        public static IReadOnlyObservableCollection<T> SubscribeWeak<T, TTarget>(this IReadOnlyObservableCollection<T> collection, TTarget target,
            Func<TTarget, CollectionChangedEventInfo<T>, bool> predicate, Action<TTarget, ItemOrArray<CollectionChangedEventInfo<T>>> onChanged,
            int delay = 0, bool listenItemChanges = true)
            where T : class
            where TTarget : class =>
            collection.SubscribeWeak(target, predicate, onChanged, delay, listenItemChanges, out _);

        public static IReadOnlyObservableCollection<T> Subscribe<T>(this IReadOnlyObservableCollection<T> collection,
            Func<CollectionChangedEventInfo<T>, bool> predicate, Action<ItemOrArray<CollectionChangedEventInfo<T>>> onChanged, int delay,
            bool listenItemChanges, out ActionToken removeToken)
            where T : class
        {
            removeToken = collection.GetOrAddComponent<CollectionObserver>().AddObserver(predicate, onChanged, delay, listenItemChanges);
            return collection;
        }

        public static IReadOnlyObservableCollection<T> Subscribe<T, TState>(this IReadOnlyObservableCollection<T> collection, TState state,
            Func<TState, CollectionChangedEventInfo<T>, bool> predicate, Action<TState, ItemOrArray<CollectionChangedEventInfo<T>>> onChanged, int delay,
            bool listenItemChanges, out ActionToken removeToken) where T : class
        {
            removeToken = collection.GetOrAddComponent<CollectionObserver>().AddObserver(predicate, onChanged, state, delay, listenItemChanges);
            return collection;
        }

        public static IReadOnlyObservableCollection<T> SubscribeWeak<T, TTarget>(this IReadOnlyObservableCollection<T> collection, TTarget target,
            Func<TTarget, CollectionChangedEventInfo<T>, bool> predicate, Action<TTarget, ItemOrArray<CollectionChangedEventInfo<T>>> onChanged,
            int delay, bool listenItemChanges, out ActionToken removeToken)
            where T : class
            where TTarget : class
        {
            removeToken = collection.GetOrAddComponent<CollectionObserver>().AddObserverWeak(target, predicate, onChanged, delay, listenItemChanges);
            return collection;
        }

        public static IReadOnlyObservableCollection<T> AsReadOnlyObservableCollection<T>(this IReadOnlyObservableCollection<T> collection, bool disposeSourceOnDispose = false) =>
            new ReadOnlyObservableCollection<T>(collection, 0, disposeSourceOnDispose);

        public static DecoratorsConfiguration Select<T, TResult>(this DecoratorsConfiguration configuration, Func<T, TResult?, TResult?> selector,
            Action<T, TResult>? cleanup = null, IEqualityComparer<TResult?>? comparer = null) where T : notnull where TResult : class? =>
            configuration.Select(selector, cleanup, comparer, out _);

        public static DecoratorsConfiguration Select<T, TResult>(this DecoratorsConfiguration configuration, Func<T, TResult?, TResult?> selector, Action<T, TResult>? cleanup,
            IEqualityComparer<TResult?>? comparer, out ConvertCollectionDecorator<T, TResult> decorator) where T : notnull where TResult : class?
        {
            decorator = new ConvertCollectionDecorator<T, TResult>(selector, cleanup, comparer, configuration.Priority);
            return configuration.Add(decorator);
        }

        public static DecoratorsConfiguration SelectImmutable<T, TResult>(this DecoratorsConfiguration configuration, Func<T, TResult> selector) where TResult : class?
            => configuration.SelectImmutable(selector, out _);

        public static DecoratorsConfiguration SelectImmutable<T, TResult>(this DecoratorsConfiguration configuration, Func<T, TResult> selector,
            out ConvertImmutableCollectionDecorator<T, TResult> decorator) where TResult : class?
        {
            decorator = new ConvertImmutableCollectionDecorator<T, TResult>(selector, configuration.Priority);
            return configuration.Add(decorator);
        }

        public static DecoratorsConfiguration Where<T>(this DecoratorsConfiguration configuration, Func<T, bool> filter, bool nullItemResult = false) =>
            configuration.Where(filter, nullItemResult, out _);

        public static DecoratorsConfiguration Where<T>(this DecoratorsConfiguration configuration, Func<T, bool> filter,
            bool nullItemResult, out FilterCollectionDecorator<T> decorator)
        {
            Should.NotBeNull(filter, nameof(filter));
            decorator = new FilterCollectionDecorator<T>(filter, configuration.Priority) { NullItemResult = nullItemResult };
            return configuration.Add(decorator);
        }

        public static DecoratorsConfiguration Take<T>(this DecoratorsConfiguration configuration, int limit, Func<T, bool>? condition = null) where T : notnull =>
            configuration.Take(limit, condition, out _);

        public static DecoratorsConfiguration Take<T>(this DecoratorsConfiguration configuration, int limit, Func<T, bool>? condition,
            out LimitCollectionDecorator<T> decorator) where T : notnull
        {
            decorator = new LimitCollectionDecorator<T>(limit, condition, configuration.Priority);
            return configuration.Add(decorator);
        }

        public static DecoratorsConfiguration OrderBy<T>(this DecoratorsConfiguration configuration, IComparer<T> comparer) => configuration.OrderBy(comparer, out _);

        public static DecoratorsConfiguration OrderBy<T>(this DecoratorsConfiguration configuration, IComparer<T> comparer,
            out SortCollectionDecorator decorator)
        {
            Should.NotBeNull(comparer, nameof(comparer));
            decorator = new SortCollectionDecorator(comparer as IComparer<object?> ?? new WrapperObjectComparer<T>(comparer), configuration.Priority);
            return configuration.Add(decorator);
        }

        public static DecoratorsConfiguration SelectMany<T>(this DecoratorsConfiguration configuration, Func<T, IEnumerable> selector, bool decoratedItems = true)
            where T : class =>
            configuration.SelectMany(selector, decoratedItems, out _);

        public static DecoratorsConfiguration SelectMany<T>(this DecoratorsConfiguration configuration, Func<T, IEnumerable> selector, bool decoratedItems,
            out FlattenCollectionDecorator<T> decorator) where T : class
        {
            Should.NotBeNull(selector, nameof(selector));
            return configuration.SelectMany(decoratedItems ? selector.SelectManyDecorated : selector.SelectMany, out decorator);
        }

        public static DecoratorsConfiguration SelectMany<T>(this DecoratorsConfiguration configuration, Func<T, FlattenItemInfo> selector) where T : class =>
            configuration.SelectMany(selector, out _);

        public static DecoratorsConfiguration SelectMany<T>(this DecoratorsConfiguration configuration, Func<T, FlattenItemInfo> selector,
            out FlattenCollectionDecorator<T> decorator) where T : class
        {
            decorator = new FlattenCollectionDecorator<T>(selector, configuration.Priority);
            return configuration.Add(decorator);
        }

        public static DecoratorsConfiguration GroupBy<T, TKey>(this DecoratorsConfiguration configuration, Func<T, TKey?> getGroup, IComparer<TKey>? groupComparer = null,
            IEqualityComparer<TKey>? equalityComparer = null, bool flatten = true) where TKey : class
        {
            configuration = configuration.GroupByRaw(getGroup, (group, items, action, item, args) =>
                                         {
                                             if (group is ICollectionGroup<T> g && (action != CollectionGroupChangedAction.GroupRemoved || !g.TryCleanup()))
                                                 action.ApplyChangesTo(g.Items, items, item, args);
                                         }, equalityComparer)
                                         .Where<T>(_ => false);
            if (groupComparer != null)
                configuration = configuration.OrderBy(groupComparer);
            if (flatten)
                return configuration.SelectMany<TKey>(group => new FlattenItemInfo((group as ICollectionGroup<T>)?.Items));
            return configuration;
        }

        public static DecoratorsConfiguration GroupByRaw<T, TKey>(this DecoratorsConfiguration configuration, Func<T, TKey?> getGroup,
            GroupCollectionDecorator<T, TKey>.UpdateGroupDelegate? updateGroup, IEqualityComparer<TKey>? equalityComparer = null)
            where TKey : class => configuration.GroupByRaw(getGroup, updateGroup, equalityComparer, out _);

        public static DecoratorsConfiguration GroupByRaw<T, TKey>(this DecoratorsConfiguration configuration, Func<T, TKey?> getGroup,
            GroupCollectionDecorator<T, TKey>.UpdateGroupDelegate? updateGroup, IEqualityComparer<TKey>? equalityComparer, out GroupCollectionDecorator<T, TKey> decorator)
            where TKey : class
        {
            decorator = new GroupCollectionDecorator<T, TKey>(getGroup, updateGroup, equalityComparer, configuration.Priority);
            return configuration.Add(decorator);
        }

        public static DecoratorsConfiguration WithHeaderFooter(this DecoratorsConfiguration configuration, ItemOrIReadOnlyList<object> header,
            ItemOrIReadOnlyList<object> footer = default) => configuration.WithHeaderFooter(header, footer, out _);

        public static DecoratorsConfiguration WithHeaderFooter(this DecoratorsConfiguration configuration, ItemOrIReadOnlyList<object> header,
            ItemOrIReadOnlyList<object> footer, out HeaderFooterCollectionDecorator decorator)
        {
            decorator = new HeaderFooterCollectionDecorator(configuration.Priority) { Header = header, Footer = footer };
            return configuration.Add(decorator);
        }

        public static DecoratorsConfiguration PinHeaderFooter<T>(this DecoratorsConfiguration configuration, Func<T, bool?> isHeaderOrFooter,
            IComparer<T>? headerComparer = null, IComparer<T>? footerComparer = null) where T : notnull =>
            configuration.PinHeaderFooter(isHeaderOrFooter, headerComparer, footerComparer, out _);

        public static DecoratorsConfiguration PinHeaderFooter<T>(this DecoratorsConfiguration configuration, Func<T, bool?> isHeaderOrFooter,
            IComparer<T>? headerComparer, IComparer<T>? footerComparer, out PinHeaderFooterCollectionDecorator<T> decorator) where T : notnull
        {
            decorator = new PinHeaderFooterCollectionDecorator<T>(isHeaderOrFooter, headerComparer, footerComparer, configuration.Priority);
            return configuration.Add(decorator);
        }

        public static DecoratorsConfiguration Prepend(this DecoratorsConfiguration configuration, ItemOrIReadOnlyList<object> items) =>
            configuration.WithHeaderFooter(items, default, out _);

        public static DecoratorsConfiguration Append(this DecoratorsConfiguration configuration, ItemOrIReadOnlyList<object> items) =>
            configuration.WithHeaderFooter(default, items, out _);

        public static DecoratorsConfiguration Concat(this DecoratorsConfiguration configuration, IEnumerable items, bool decoratedItemsSource = true)
        {
            Should.NotBeNull(items, nameof(items));
            return configuration.WithHeaderFooter(default, new ItemOrIReadOnlyList<object>(items), out _)
                                .SelectMany<IEnumerable>(decoratedItemsSource ? items.FlattenDecorated : items.Flatten);
        }

        public static DecoratorsConfiguration Subscribe<T>(this DecoratorsConfiguration configuration, Action<IEnumerable<T>, T> onAdded, Action<IEnumerable<T>, T> onRemoved,
            Action<IEnumerable<T>, T>? onChanged = null) where T : class => configuration.Subscribe(onAdded, onRemoved, onChanged, out _);

        public static DecoratorsConfiguration Subscribe<T>(this DecoratorsConfiguration configuration, Action<IEnumerable<T>, T> onAdded, Action<IEnumerable<T>, T> onRemoved,
            Action<IEnumerable<T>, T>? onChanged, out ActionToken removeToken)
            where T : class
        {
            var decorator = new ConvertHandlerClosure<T>(configuration.Collection, onAdded, onRemoved, onChanged, configuration.Priority).Decorator;
            removeToken = configuration.Collection.AddComponent(decorator);
            return configuration.UpdatePriority();
        }

        public static DecoratorsConfiguration Subscribe<T>(this DecoratorsConfiguration configuration,
            Func<CollectionChangedEventInfo<T>, bool> predicate, Action<ItemOrArray<CollectionChangedEventInfo<T>>> onChanged, int delay = 0,
            bool listenItemChanges = true)
            where T : class =>
            configuration.Subscribe(predicate, onChanged, delay, listenItemChanges, out _);

        public static DecoratorsConfiguration Subscribe<T, TState>(this DecoratorsConfiguration configuration,
            Func<TState, CollectionChangedEventInfo<T>, bool> predicate, Action<TState, ItemOrArray<CollectionChangedEventInfo<T>>> onChanged,
            TState state, int delay = 0, bool listenItemChanges = true) where T : class =>
            configuration.Subscribe(predicate, onChanged, state, delay, listenItemChanges, out _);

        public static DecoratorsConfiguration SubscribeWeak<T, TTarget>(this DecoratorsConfiguration configuration, TTarget target,
            Func<TTarget, CollectionChangedEventInfo<T>, bool> predicate, Action<TTarget, ItemOrArray<CollectionChangedEventInfo<T>>> onChanged,
            int delay = 0, bool listenItemChanges = true)
            where T : class
            where TTarget : class =>
            configuration.SubscribeWeak(target, predicate, onChanged, delay, listenItemChanges, out _);

        public static DecoratorsConfiguration Subscribe<T>(this DecoratorsConfiguration configuration,
            Func<CollectionChangedEventInfo<T>, bool> predicate, Action<ItemOrArray<CollectionChangedEventInfo<T>>> onChanged, int delay,
            bool listenItemChanges, out ActionToken removeToken)
            where T : class
        {
            removeToken = configuration.GetObserverCollectionDecorator().AddObserver(predicate, onChanged, delay, listenItemChanges);
            return configuration;
        }

        public static DecoratorsConfiguration Subscribe<T, TState>(this DecoratorsConfiguration configuration,
            Func<TState, CollectionChangedEventInfo<T>, bool> predicate, Action<TState, ItemOrArray<CollectionChangedEventInfo<T>>> onChanged,
            TState state, int delay, bool listenItemChanges, out ActionToken removeToken) where T : class
        {
            removeToken = configuration.GetObserverCollectionDecorator().AddObserver(predicate, onChanged, state, delay, listenItemChanges);
            return configuration;
        }

        public static DecoratorsConfiguration SubscribeWeak<T, TTarget>(this DecoratorsConfiguration configuration, TTarget target,
            Func<TTarget, CollectionChangedEventInfo<T>, bool> predicate, Action<TTarget, ItemOrArray<CollectionChangedEventInfo<T>>> onChanged,
            int delay, bool listenItemChanges, out ActionToken removeToken)
            where T : class
            where TTarget : class
        {
            removeToken = configuration.GetObserverCollectionDecorator().AddObserverWeak(target, predicate, onChanged, delay, listenItemChanges);
            return configuration;
        }

        public static DecoratorsConfiguration Bind(this DecoratorsConfiguration configuration, out IReadOnlyObservableCollection<object?> collection,
            bool disposeSourceOnDispose = false) =>
            configuration.Bind<object?>(out collection, disposeSourceOnDispose);

        public static DecoratorsConfiguration Bind<T>(this DecoratorsConfiguration configuration, out IReadOnlyObservableCollection<T> collection,
            bool disposeSourceOnDispose = false)
        {
            collection = new DecoratedReadOnlyObservableCollection<T>(configuration.Collection, configuration.Priority, disposeSourceOnDispose);
            return configuration.UpdatePriority();
        }

        public static DecoratorsConfiguration AutoRefreshOnPropertyChanged<T>(this DecoratorsConfiguration configuration, ItemOrArray<string> members,
            object? args = null, int delay = 0) where T : class => configuration.AutoRefreshOnPropertyChanged<T>(members, args, delay, out _);

        public static DecoratorsConfiguration AutoRefreshOnPropertyChanged<T>(this DecoratorsConfiguration configuration, ItemOrArray<string> members,
            object? args, int delay, out ActionToken removeToken)
            where T : class
        {
            removeToken = configuration.GetObserverCollectionDecorator().AddObserver<T, (ItemOrArray<string> members, object? args)>((s, info) =>
            {
                if (info.IsCollectionEvent)
                    return false;

                foreach (var member in s.members)
                {
                    if (info.IsMemberChanged(member))
                        return true;
                }

                return false;
            }, (s, info) =>
            {
                foreach (var eventInfo in info)
                    eventInfo.Collection?.RaiseItemChanged(eventInfo.Item, s.args);
            }, (members, args), delay, true);
            return configuration;
        }

        public static void ApplyChangesTo<T>(this CollectionGroupChangedAction action, IList<T> items, IReadOnlyList<T> groupItems, T? item, object? args)
        {
            switch (action)
            {
                case CollectionGroupChangedAction.GroupRemoved:
                    items.Clear();
                    break;
                case CollectionGroupChangedAction.ItemAdded:
                    items.Add(item!);
                    break;
                case CollectionGroupChangedAction.ItemRemoved:
                    items.Remove(item!);
                    break;
                case CollectionGroupChangedAction.Reset:
                    items.Reset(groupItems);
                    break;
                case CollectionGroupChangedAction.ItemChanged:
                    (items as IReadOnlyObservableCollection)?.RaiseItemChanged(item, args);
                    break;
            }
        }

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
                source = new[] { (T)source, value };
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

        internal static ActionToken BatchUpdateDecorators(this IReadOnlyObservableCollection collection, ICollectionBatchUpdateManagerComponent manager)
        {
            manager.BeginBatchUpdate(collection, BatchUpdateType.Decorators);
            return ActionToken.FromDelegate((m, c) => ((ICollectionBatchUpdateManagerComponent)m!).EndBatchUpdate((IReadOnlyObservableCollection)c!, BatchUpdateType.Decorators),
                manager, collection);
        }

        private static DecoratedCollectionObserver GetObserverCollectionDecorator(this ref DecoratorsConfiguration configuration)
        {
            var components = configuration.Collection.GetComponents<DecoratedCollectionObserver>();
            if (components.Count != 0)
            {
                var component = components[components.Count - 1];
                if (component.Priority == configuration.Priority - configuration.Step)
                    return component;
            }

            var decorator = new DecoratedCollectionObserver { Priority = configuration.Priority };
            configuration = configuration.Add(decorator);
            return decorator;
        }

        private static FlattenItemInfo FlattenDecorated(this IEnumerable enumerable, IEnumerable value)
        {
            if (ReferenceEquals(enumerable, value))
                return new FlattenItemInfo(enumerable, true);
            return default;
        }

        private static FlattenItemInfo Flatten(this IEnumerable enumerable, IEnumerable value)
        {
            if (ReferenceEquals(enumerable, value))
                return new FlattenItemInfo(enumerable, false);
            return default;
        }

        private static FlattenItemInfo SelectManyDecorated<T>(this Func<T, IEnumerable> selector, T item) => new(selector(item));

        private static FlattenItemInfo SelectMany<T>(this Func<T, IEnumerable> selector, T item) => new(selector(item), false);

        private sealed class ConvertHandlerClosure<T> : IEnumerable<T> where T : class
        {
            private readonly IReadOnlyObservableCollection _collection;
            private readonly Action<IEnumerable<T>, T> _onAdded;
            private readonly Action<IEnumerable<T>, T> _onRemoved;
            private readonly Action<IEnumerable<T>, T>? _onChanged;

            public ConvertHandlerClosure(IReadOnlyObservableCollection collection, Action<IEnumerable<T>, T> onAdded, Action<IEnumerable<T>, T> onRemoved,
                Action<IEnumerable<T>, T>? onChanged, int priority)
            {
                Should.NotBeNull(onAdded, nameof(onAdded));
                Should.NotBeNull(onRemoved, nameof(onRemoved));
                _collection = collection;
                _onAdded = onAdded;
                _onRemoved = onRemoved;
                _onChanged = onChanged;
                Decorator = new ConvertCollectionDecorator<T, T>(Convert, Cleanup, InternalEqualityComparer.Reference!, priority, false);
            }

            public ConvertCollectionDecorator<T, T> Decorator { get; }

            public IEnumerator<T> GetEnumerator() => _collection.DecoratedItems<object?>(Decorator)?.OfType<T>().GetEnumerator() ?? Default.EmptyEnumerator<T>();

            private T Convert(T item, T? oldItem)
            {
                if (ReferenceEquals(item, oldItem))
                    _onChanged?.Invoke(this, item);
                else
                    _onAdded(this, item);
                return item;
            }

            private void Cleanup(T item, T? oldItem) => _onRemoved(this, item);

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
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