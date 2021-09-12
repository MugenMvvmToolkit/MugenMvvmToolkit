using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using MugenMvvm.Collections;
using MugenMvvm.Collections.Components;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Extensions
{
    public static partial class MugenExtensions
    {
        public static IReadOnlyObservableCollection<T> BindToSource<T>(this IReadOnlyObservableCollection<T> collection, bool disposeSourceOnDispose = false) =>
            new ReadOnlyObservableCollection<T>(collection, 0, disposeSourceOnDispose);

        public static IReadOnlyObservableCollection<object?> Bind(this IReadOnlyObservableCollection collection, bool disposeSourceOnDispose = false) =>
            collection.Bind<object?>(disposeSourceOnDispose);

        public static IReadOnlyObservableCollection<T> Bind<T>(this IReadOnlyObservableCollection collection, bool disposeSourceOnDispose = false)
        {
            collection.ConfigureDecorators().Bind<T>(out var result, disposeSourceOnDispose);
            return result;
        }

        public static IReadOnlyObservableCollection<object?> CreateDerivedCollection<T>(this IEnumerable<T> items, bool disposeSourceOnDispose = false) where T : class
        {
            Should.NotBeNull(items, nameof(items));
            if (items is IReadOnlyObservableCollection collection)
                return collection.Bind(disposeSourceOnDispose);

            var objects = new SynchronizedObservableCollection<T>();
            objects.Reset(items);
            return objects;
        }

        public static IReadOnlyObservableCollection<T> CreateDerivedCollectionSource<T>(this IEnumerable<T> items, bool disposeSourceOnDispose = false) where T : class
        {
            Should.NotBeNull(items, nameof(items));
            if (items is IReadOnlyObservableCollection<T> collection)
                return collection.BindToSource(disposeSourceOnDispose);

            var objects = new SynchronizedObservableCollection<T>();
            objects.Reset(items);
            return objects;
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
                var observableCollection = (IReadOnlyObservableCollection) c!;
                observableCollection.GetBatchUpdateManager().EndBatchUpdate(observableCollection, (BatchUpdateType) t!);
            }, collection, batchUpdateType);
        }

        public static void RaiseItemChanged(this IReadOnlyObservableCollection collection, object? item, object? args = null)
        {
            Should.NotBeNull(collection, nameof(collection));
            collection.GetComponentOptional<ICollectionDecoratorManagerComponent>()?.RaiseItemChanged(collection, item, args);
        }

        public static DecoratorsConfiguration DisposeWith<T>(this DecoratorsConfiguration configuration, IComponentOwner<T> owner) where T : class, IDisposable
        {
            configuration.Collection.DisposeWith(owner);
            return configuration;
        }

        public static DecoratorsConfiguration DisposeWith(this DecoratorsConfiguration configuration, IHasDisposeCallback owner)
        {
            configuration.Collection.DisposeWith(owner);
            return configuration;
        }

        public static IReadOnlyObservableCollection<object?> AsCollection(this DecoratorsConfiguration configuration) =>
            (IReadOnlyObservableCollection<object?>) configuration.Collection;

        public static IReadOnlyObservableCollection<T> AsCollection<T>(this DecoratorsConfiguration configuration) => (IReadOnlyObservableCollection<T>) configuration.Collection;

        public static SynchronizedObservableCollection<T> AsSynchronizedCollection<T>(this DecoratorsConfiguration configuration) =>
            (SynchronizedObservableCollection<T>) configuration.Collection;

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
            decorator = new FilterCollectionDecorator<T>(filter, configuration.Priority) {NullItemResult = nullItemResult};
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

        public static DecoratorsConfiguration OrderBy<T>(this DecoratorsConfiguration configuration, IComparer<T> comparer, out SortCollectionDecorator decorator)
        {
            Should.NotBeNull(comparer, nameof(comparer));
            decorator = new SortCollectionDecorator(comparer as IComparer<object?> ?? new WrapperObjectComparer<T>(comparer), configuration.Priority);
            return configuration.Add(decorator);
        }

        public static DecoratorsConfiguration SelectMany<T>(this DecoratorsConfiguration configuration, Func<T, IEnumerable?> selector, bool decoratedItems = true)
            where T : class => configuration.SelectMany(selector, decoratedItems, out _);

        public static DecoratorsConfiguration SelectMany<T>(this DecoratorsConfiguration configuration, Func<T, IEnumerable?> selector, bool decoratedItems,
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

        public static DecoratorsConfiguration GroupBy<T, TGroup>(this DecoratorsConfiguration configuration, Func<T, TGroup?> getGroup, IComparer<TGroup>? groupComparer = null,
            IEqualityComparer<TGroup>? equalityComparer = null, bool flatten = true)
            where TGroup : class
            where T : notnull
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
                return configuration.SelectMany<TGroup>(group => new FlattenItemInfo((group as ICollectionGroup<T>)?.Items, true));
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
            decorator = new HeaderFooterCollectionDecorator(configuration.Priority) {Header = header, Footer = footer};
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

        public static DecoratorsConfiguration Subscribe<T, TState>(this DecoratorsConfiguration configuration,
            Func<IReadOnlyDictionary<T, (TState state, int count)>, T, TState?, int, bool, TState> onAdded,
            Func<IReadOnlyDictionary<T, (TState state, int count)>, T, TState, int, bool, TState> onRemoved,
            Func<IReadOnlyDictionary<T, (TState state, int count)>, T, TState, int, bool, object?, TState>? onChanged = null,
            Action<IReadOnlyDictionary<T, (TState state, int count)>>? onReset = null,
            IEqualityComparer<T>? comparer = null)
            where T : notnull =>
            configuration.Subscribe(onAdded, onRemoved, onChanged, onReset, comparer, out _);

        public static DecoratorsConfiguration Subscribe<T, TState>(this DecoratorsConfiguration configuration,
            Func<IReadOnlyDictionary<T, (TState state, int count)>, T, TState?, int, bool, TState> onAdded,
            Func<IReadOnlyDictionary<T, (TState state, int count)>, T, TState, int, bool, TState> onRemoved,
            Func<IReadOnlyDictionary<T, (TState state, int count)>, T, TState, int, bool, object?, TState>? onChanged,
            Action<IReadOnlyDictionary<T, (TState state, int count)>>? onReset,
            IEqualityComparer<T>? comparer, out ActionToken removeToken)
            where T : notnull
        {
            var decorator = new TrackerCollectionDecorator<T, TState>(onAdded, onRemoved, onChanged, onReset, comparer, configuration.Priority);
            return configuration.Add(decorator, null, out removeToken);
        }

        public static DecoratorsConfiguration Bind(this DecoratorsConfiguration configuration, out IReadOnlyObservableCollection<object?> collection,
            bool disposeSourceOnDispose = false) => configuration.Bind<object?>(out collection, disposeSourceOnDispose);

        public static DecoratorsConfiguration Bind<T>(this DecoratorsConfiguration configuration, out IReadOnlyObservableCollection<T> collection,
            bool disposeSourceOnDispose = false)
        {
            collection = new DecoratedReadOnlyObservableCollection<T>(configuration.Collection, configuration.Priority, disposeSourceOnDispose);
            return configuration.UpdatePriority();
        }

        public static DecoratorsConfiguration TrackSelectedItem<T>(this DecoratorsConfiguration configuration, Func<T?> getSelectedItem, Action<T?> setSelectedItem,
            Func<IEnumerable<T>, T?, T?>? getDefault = null, IEqualityComparer<T>? comparer = null) where T : class =>
            configuration.TrackSelectedItem(getSelectedItem, setSelectedItem, getDefault, comparer, out _);

        public static DecoratorsConfiguration TrackSelectedItem<T>(this DecoratorsConfiguration configuration, Func<T?> getSelectedItem, Action<T?> setSelectedItem,
            Func<IEnumerable<T>, T?, T?>? getDefault, IEqualityComparer<T>? comparer, out ActionToken removeToken) where T : class
        {
            Should.NotBeNull(getSelectedItem, nameof(getSelectedItem));
            Should.NotBeNull(setSelectedItem, nameof(setSelectedItem));
            return configuration.Subscribe<T, object?>((items, item, _, _, isReset) =>
            {
                if (!isReset)
                {
                    var selectedItem = getSelectedItem();
                    if (selectedItem == null || !(comparer ?? EqualityComparer<T>.Default).Equals(selectedItem, item) && !items.ContainsKey(selectedItem))
                        setSelectedItem(item);
                }

                return null;
            }, (items, _, _, _, isReset) =>
            {
                if (!isReset)
                {
                    var selectedItem = getSelectedItem();
                    if (selectedItem == null || !items.ContainsKey(selectedItem))
                        setSelectedItem(getDefault?.Invoke(items.Keys, selectedItem) ?? Enumerable.FirstOrDefault(items).Key);
                }

                return null;
            }, null, items =>
            {
                var selectedItem = getSelectedItem();
                if (selectedItem == null || !items.ContainsKey(selectedItem))
                    setSelectedItem(getDefault?.Invoke(items.Keys, selectedItem) ?? Enumerable.FirstOrDefault(items).Key);
            }, comparer, out removeToken);
        }

        public static DecoratorsConfiguration AutoRefreshOnPropertyChanged<T>(this DecoratorsConfiguration configuration, ItemOrArray<string> members, object? args = null)
            where T : class => configuration.AutoRefreshOnPropertyChanged<T>(members, args, out _);

        public static DecoratorsConfiguration AutoRefreshOnPropertyChanged<T>(this DecoratorsConfiguration configuration, ItemOrArray<string> members,
            object? args, out ActionToken removeToken) where T : class
        {
            var collection = configuration.Collection;
            PropertyChangedEventHandler handler = (sender, e) =>
            {
                if (sender != null && string.IsNullOrEmpty(e.PropertyName) || members.Contains(e.PropertyName!))
                    collection.RaiseItemChanged(sender, args ?? e);
            };
            return configuration.Subscribe<T, PropertyChangedEventHandler?>((_, item, _, count, _) =>
            {
                if (count == 1 && item is INotifyPropertyChanged notifyPropertyChanged)
                {
                    notifyPropertyChanged.PropertyChanged += handler;
                    return handler;
                }

                return null;
            }, (_, item, state, count, _) =>
            {
                if (count == 0 && state != null)
                    ((INotifyPropertyChanged) item).PropertyChanged -= state;
                return state;
            }, null, null, InternalEqualityComparer.Reference, out removeToken);
        }

        public static DecoratorsConfiguration AutoRefreshOnObservable<T, TSignal>(this DecoratorsConfiguration configuration, Func<T, IObservable<TSignal>> getObservable,
            object? args = null) where T : class => configuration.AutoRefreshOnObservable(getObservable, args, out _);

        public static DecoratorsConfiguration AutoRefreshOnObservable<T, TSignal>(this DecoratorsConfiguration configuration, Func<T, IObservable<TSignal>> getObservable,
            object? args, out ActionToken removeToken) where T : class
        {
            Should.NotBeNull(getObservable, nameof(getObservable));
            var collection = configuration.Collection;
            return configuration.Subscribe<T, IDisposable>((_, item, state, _, _) =>
            {
                if (state == null)
                    return getObservable(item).Subscribe(new ItemChangedObserver<TSignal>(collection, item, args));
                return state;
            }, (_, _, state, count, _) =>
            {
                if (count == 0)
                    state.Dispose();
                return state;
            }, null, null, InternalEqualityComparer.Reference, out removeToken);
        }

        public static DecoratorsConfiguration Min<T, TResult>(this DecoratorsConfiguration configuration, Func<T, TResult> selector, Action<TResult?> onChanged,
            TResult? defaultValue = default, Func<T, bool>? predicate = null, IComparer<TResult?>? comparer = null)
            where T : notnull =>
            configuration.MaxMin(selector, onChanged, defaultValue, predicate, comparer, false, out _);

        public static DecoratorsConfiguration Min<T, TResult>(this DecoratorsConfiguration configuration, Func<T, TResult> selector, Action<TResult?> onChanged,
            TResult? defaultValue, Func<T, bool>? predicate, IComparer<TResult?>? comparer, out ActionToken removeToken)
            where T : notnull =>
            configuration.MaxMin(selector, onChanged, defaultValue, predicate, comparer, false, out removeToken);

        public static DecoratorsConfiguration Max<T, TResult>(this DecoratorsConfiguration configuration, Func<T, TResult> selector, Action<TResult?> onChanged,
            TResult? defaultValue = default, Func<T, bool>? predicate = null, IComparer<TResult?>? comparer = null)
            where T : notnull =>
            configuration.MaxMin(selector, onChanged, defaultValue, predicate, comparer, true, out _);

        public static DecoratorsConfiguration Max<T, TResult>(this DecoratorsConfiguration configuration, Func<T, TResult> selector, Action<TResult?> onChanged,
            TResult? defaultValue, Func<T, bool>? predicate, IComparer<TResult?>? comparer, out ActionToken removeToken)
            where T : notnull =>
            configuration.MaxMin(selector, onChanged, defaultValue, predicate, comparer, true, out removeToken);

        public static DecoratorsConfiguration Count<T>(this DecoratorsConfiguration configuration, Action<int> onChanged, Func<T, bool>? predicate = null)
            where T : notnull => configuration.Count(onChanged, predicate, out _);

        public static DecoratorsConfiguration Count<T>(this DecoratorsConfiguration configuration, Action<int> onChanged, Func<T, bool>? predicate, out ActionToken removeToken)
            where T : notnull =>
            configuration.Accumulate(0, _ => 1, (x1, x2) => x1 + x2, (x1, x2) => x1 - x2, onChanged, predicate, out removeToken);

        public static DecoratorsConfiguration Sum<T>(this DecoratorsConfiguration configuration, Func<T, int> selector, Action<int> onChanged, Func<T, bool>? predicate = null)
            where T : notnull => configuration.Sum(selector, onChanged, predicate, out _);

        public static DecoratorsConfiguration Sum<T>(this DecoratorsConfiguration configuration, Func<T, int?> selector, Action<int?> onChanged, Func<T, bool>? predicate = null)
            where T : notnull => configuration.Sum(selector, onChanged, predicate, out _);

        public static DecoratorsConfiguration Sum<T>(this DecoratorsConfiguration configuration, Func<T, long> selector, Action<long> onChanged, Func<T, bool>? predicate = null)
            where T : notnull => configuration.Sum(selector, onChanged, predicate, out _);

        public static DecoratorsConfiguration Sum<T>(this DecoratorsConfiguration configuration, Func<T, long?> selector, Action<long?> onChanged, Func<T, bool>? predicate = null)
            where T : notnull => configuration.Sum(selector, onChanged, predicate, out _);

        public static DecoratorsConfiguration Sum<T>(this DecoratorsConfiguration configuration, Func<T, float> selector, Action<float> onChanged, Func<T, bool>? predicate = null)
            where T : notnull => configuration.Sum(selector, onChanged, predicate, out _);

        public static DecoratorsConfiguration Sum<T>(this DecoratorsConfiguration configuration, Func<T, float?> selector, Action<float?> onChanged,
            Func<T, bool>? predicate = null)
            where T : notnull => configuration.Sum(selector, onChanged, predicate, out _);

        public static DecoratorsConfiguration Sum<T>(this DecoratorsConfiguration configuration, Func<T, double> selector, Action<double> onChanged,
            Func<T, bool>? predicate = null)
            where T : notnull => configuration.Sum(selector, onChanged, predicate, out _);

        public static DecoratorsConfiguration Sum<T>(this DecoratorsConfiguration configuration, Func<T, double?> selector, Action<double?> onChanged,
            Func<T, bool>? predicate = null)
            where T : notnull => configuration.Sum(selector, onChanged, predicate, out _);

        public static DecoratorsConfiguration Sum<T>(this DecoratorsConfiguration configuration, Func<T, decimal> selector, Action<decimal> onChanged,
            Func<T, bool>? predicate = null)
            where T : notnull => configuration.Sum(selector, onChanged, predicate, out _);

        public static DecoratorsConfiguration Sum<T>(this DecoratorsConfiguration configuration, Func<T, decimal?> selector, Action<decimal?> onChanged,
            Func<T, bool>? predicate = null)
            where T : notnull => configuration.Sum(selector, onChanged, predicate, out _);

        public static DecoratorsConfiguration Sum<T>(this DecoratorsConfiguration configuration, Func<T, int> selector, Action<int> onChanged, Func<T, bool>? predicate,
            out ActionToken removeToken)
            where T : notnull =>
            configuration.Accumulate(0, selector, (x1, x2) => x1 + x2, (x1, x2) => x1 - x2, onChanged, predicate, out removeToken);

        public static DecoratorsConfiguration Sum<T>(this DecoratorsConfiguration configuration, Func<T, int?> selector, Action<int?> onChanged, Func<T, bool>? predicate,
            out ActionToken removeToken)
            where T : notnull =>
            configuration.Accumulate(0, selector, (x1, x2) => x1 + x2, (x1, x2) => x1 - x2, onChanged, predicate, out removeToken);

        public static DecoratorsConfiguration Sum<T>(this DecoratorsConfiguration configuration, Func<T, long> selector, Action<long> onChanged, Func<T, bool>? predicate,
            out ActionToken removeToken)
            where T : notnull =>
            configuration.Accumulate(0, selector, (x1, x2) => x1 + x2, (x1, x2) => x1 - x2, onChanged, predicate, out removeToken);

        public static DecoratorsConfiguration Sum<T>(this DecoratorsConfiguration configuration, Func<T, long?> selector, Action<long?> onChanged, Func<T, bool>? predicate,
            out ActionToken removeToken)
            where T : notnull =>
            configuration.Accumulate(0, selector, (x1, x2) => x1 + x2, (x1, x2) => x1 - x2, onChanged, predicate, out removeToken);

        public static DecoratorsConfiguration Sum<T>(this DecoratorsConfiguration configuration, Func<T, float> selector, Action<float> onChanged, Func<T, bool>? predicate,
            out ActionToken removeToken)
            where T : notnull =>
            configuration.Accumulate(0, selector, (x1, x2) => x1 + x2, (x1, x2) => x1 - x2, onChanged, predicate, out removeToken);

        public static DecoratorsConfiguration Sum<T>(this DecoratorsConfiguration configuration, Func<T, float?> selector, Action<float?> onChanged,
            Func<T, bool>? predicate, out ActionToken removeToken)
            where T : notnull =>
            configuration.Accumulate(0, selector, (x1, x2) => x1 + x2, (x1, x2) => x1 - x2, onChanged, predicate, out removeToken);

        public static DecoratorsConfiguration Sum<T>(this DecoratorsConfiguration configuration, Func<T, double> selector, Action<double> onChanged,
            Func<T, bool>? predicate, out ActionToken removeToken)
            where T : notnull =>
            configuration.Accumulate(0, selector, (x1, x2) => x1 + x2, (x1, x2) => x1 - x2, onChanged, predicate, out removeToken);

        public static DecoratorsConfiguration Sum<T>(this DecoratorsConfiguration configuration, Func<T, double?> selector, Action<double?> onChanged,
            Func<T, bool>? predicate, out ActionToken removeToken)
            where T : notnull =>
            configuration.Accumulate(0, selector, (x1, x2) => x1 + x2, (x1, x2) => x1 - x2, onChanged, predicate, out removeToken);

        public static DecoratorsConfiguration Sum<T>(this DecoratorsConfiguration configuration, Func<T, decimal> selector, Action<decimal> onChanged,
            Func<T, bool>? predicate, out ActionToken removeToken)
            where T : notnull =>
            configuration.Accumulate(0, selector, (x1, x2) => x1 + x2, (x1, x2) => x1 - x2, onChanged, predicate, out removeToken);

        public static DecoratorsConfiguration Sum<T>(this DecoratorsConfiguration configuration, Func<T, decimal?> selector, Action<decimal?> onChanged,
            Func<T, bool>? predicate, out ActionToken removeToken)
            where T : notnull =>
            configuration.Accumulate(0, selector, (x1, x2) => x1 + x2, (x1, x2) => x1 - x2, onChanged, predicate, out removeToken);

        public static DecoratorsConfiguration Accumulate<T, TResult>(this DecoratorsConfiguration configuration, TResult seed, Func<T, TResult> selector,
            Func<TResult, TResult, TResult> add, Func<TResult, TResult, TResult> remove, Action<TResult> onChanged, Func<T, bool>? predicate = null)
            where T : notnull =>
            configuration.Accumulate(seed, selector, add, remove, onChanged, predicate, out _);

        public static DecoratorsConfiguration Accumulate<T, TResult>(this DecoratorsConfiguration configuration, TResult seed, Func<T, TResult> selector,
            Func<TResult, TResult, TResult> add, Func<TResult, TResult, TResult> remove, Action<TResult> onChanged, Func<T, bool>? predicate, out ActionToken removeToken)
            where T : notnull
        {
            Should.NotBeNull(selector, nameof(selector));
            Should.NotBeNull(add, nameof(add));
            Should.NotBeNull(remove, nameof(remove));
            Should.NotBeNull(onChanged, nameof(onChanged));
            var closure = new AccumulateClosure<T, TResult>(seed, selector, add, remove, onChanged, predicate);
            return configuration.Subscribe<T, (TResult, bool)>(closure.OnAdded, closure.OnRemoved, closure.OnChanged, closure.OnReset, null, out removeToken);
        }

        public static void ApplyChangesTo<T>(this CollectionGroupChangedAction action, IList<T> items, IEnumerable<T> groupItems, T? item, object? args,
            bool checkDisposable = true)
        {
            switch (action)
            {
                case CollectionGroupChangedAction.GroupRemoved:
                    if (!checkDisposable || items is not IHasDisposeState {IsDisposed: true})
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

        public static IReadOnlyDictionary<TKey, TValue> Merge<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, IReadOnlyDictionary<TKey, TValue> values,
            IEqualityComparer<TKey>? comparer = null)
            where TKey : notnull
        {
            Should.NotBeNull(dictionary, nameof(dictionary));
            Should.NotBeNull(values, nameof(values));
            if (dictionary.Count == 0)
                return values;
            if (values.Count == 0)
                return dictionary;

#if NET461
            var result = new Dictionary<TKey, TValue>(dictionary.Count, comparer ?? EqualityComparer<TKey>.Default);
            foreach (var value in dictionary)
                result[value.Key] = value.Value;
#else
            var result = new Dictionary<TKey, TValue>(dictionary, comparer ?? EqualityComparer<TKey>.Default);
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

        internal static void FindAllIndexOf(this IEnumerable<object?> items, object? item, bool ignoreDuplicates, ref ItemOrListEditor<int> indexes)
        {
            if (ignoreDuplicates)
            {
                if (items is IList<object?> list)
                {
                    var indexOf = list.IndexOf(item);
                    if (indexOf >= 0)
                        indexes.Add(indexOf);
                    return;
                }
            }

            var index = 0;
            foreach (var value in items)
            {
                if (Equals(item, value))
                {
                    indexes.Add(index);
                    if (ignoreDuplicates)
                        return;
                }

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
            return ActionToken.FromDelegate((m, c) => ((ICollectionBatchUpdateManagerComponent) m!).EndBatchUpdate((IReadOnlyObservableCollection) c!, BatchUpdateType.Decorators),
                manager, collection);
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

        private static FlattenItemInfo SelectManyDecorated<T>(this Func<T, IEnumerable?> selector, T item) => new(selector(item), true);

        private static FlattenItemInfo SelectMany<T>(this Func<T, IEnumerable?> selector, T item) => new(selector(item), false);

        private static DecoratorsConfiguration MaxMin<T, TResult>(this DecoratorsConfiguration configuration, Func<T, TResult> selector, Action<TResult?> onChanged,
            TResult? defaultValue, Func<T, bool>? predicate, IComparer<TResult?>? comparer, bool isMax, out ActionToken removeToken)
            where T : notnull
        {
            Should.NotBeNull(selector, nameof(selector));
            Should.NotBeNull(onChanged, nameof(onChanged));
            var closure = new MaxMinClosure<T, TResult>(defaultValue, selector, onChanged, comparer, isMax, predicate);
            return configuration.Subscribe<T, (TResult?, bool)>(closure.OnAdded, closure.OnRemoved, closure.OnChanged, closure.OnReset, null, out removeToken);
        }

        private sealed class ItemChangedObserver<T> : IObserver<T>
        {
            private readonly IReadOnlyObservableCollection _collection;
            private readonly object _item;
            private readonly object? _args;
            private int _isStopped;

            public ItemChangedObserver(IReadOnlyObservableCollection collection, object item, object? args = null)
            {
                _collection = collection;
                _item = item;
                _args = args;
            }

            public void OnCompleted() => Interlocked.Exchange(ref _isStopped, 1);

            public void OnError(Exception error) => Interlocked.Exchange(ref _isStopped, 1);

            public void OnNext(T value)
            {
                if (Volatile.Read(ref _isStopped) == 0)
                    _collection.RaiseItemChanged(_item, _args);
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