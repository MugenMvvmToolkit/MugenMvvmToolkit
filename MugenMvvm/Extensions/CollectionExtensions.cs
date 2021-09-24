using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using JetBrains.Annotations;
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
    public static partial class MugenExtensions //todo add join
    {
        public static IReadOnlyObservableCollection<T> BindToSource<T>(this IReadOnlyObservableCollection<T> collection, bool disposeSourceOnDispose = false, bool isWeak = true) =>
            new Collections.ReadOnlyObservableCollection<T>(collection, 0, disposeSourceOnDispose, isWeak);

        public static IReadOnlyObservableCollection<object?> Bind(this IReadOnlyObservableCollection collection, bool disposeSourceOnDispose = false, bool isWeak = true,
            bool materialize = false) => collection.Bind<object?>(disposeSourceOnDispose, isWeak, materialize);

        public static IReadOnlyObservableCollection<T> Bind<T>(this IReadOnlyObservableCollection collection, bool disposeSourceOnDispose = false, bool isWeak = true,
            bool materialize = false)
        {
            collection.ConfigureDecorators<T>().Bind(out var result, disposeSourceOnDispose, isWeak, materialize);
            return result;
        }

        public static IReadOnlyObservableCollection<object?> CreateDerivedCollection<T>(this IEnumerable<T> items, bool disposeSourceOnDispose = false, bool isWeak = true,
            bool materialize = false)
            where T : class
        {
            Should.NotBeNull(items, nameof(items));
            if (items is IReadOnlyObservableCollection collection)
                return collection.Bind(disposeSourceOnDispose, isWeak, materialize);

            var objects = new SynchronizedObservableCollection<T>();
            objects.Reset(items);
            return objects;
        }

        public static IReadOnlyObservableCollection<T> CreateDerivedCollectionSource<T>(this IEnumerable<T> items, bool disposeSourceOnDispose = false, bool isWeak = true)
            where T : class
        {
            Should.NotBeNull(items, nameof(items));
            if (items is IReadOnlyObservableCollection<T> collection)
                return collection.BindToSource(disposeSourceOnDispose, isWeak);

            var objects = new SynchronizedObservableCollection<T>();
            objects.Reset(items);
            return objects;
        }

        public static DecoratorsConfiguration<T> ConfigureDecorators<T>(this IReadOnlyObservableCollection<T> genericCollection, int? priority = null, int step = 10) =>
            ConfigureDecorators<T>(collection: genericCollection, priority, step);

        public static DecoratorsConfiguration<object?> ConfigureDecorators(this IReadOnlyObservableCollection collection, int? priority = null, int step = 10) =>
            collection.ConfigureDecorators<object?>(priority, step);

        public static DecoratorsConfiguration<T> ConfigureDecorators<T>(this IReadOnlyObservableCollection collection, int? priority = null, int step = 10)
        {
            Should.NotBeNull(collection, nameof(collection));
            if (priority == null)
            {
                var array = collection.GetComponents<ICollectionDecorator>();
                priority = array.Count == 0 ? 0 : GetComponentPriority(array[array.Count - 1]) - step;
            }

            return new DecoratorsConfiguration<T>(collection, priority.Value, step);
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

        public static void OnReset(this ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ICollectionDecorator decorator)
        {
            Should.NotBeNull(decoratorManager, nameof(decoratorManager));
            Should.NotBeNull(collection, nameof(collection));
            Should.NotBeNull(decorator, nameof(decorator));
            decoratorManager.OnReset(collection, decorator, decoratorManager.Decorate(collection, decorator));
        }

        public static DecoratorsConfiguration<T> SynchronizeLocker<T>(this DecoratorsConfiguration<T> configuration, IReadOnlyObservableCollection collection) =>
            configuration.SynchronizeLocker(collection, out _);

        public static DecoratorsConfiguration<T> SynchronizeLocker<T>(this DecoratorsConfiguration<T> configuration, IReadOnlyObservableCollection collection,
            out ActionToken removeToken)
        {
            removeToken = configuration.Collection.SynchronizeLockerWith(collection);
            return configuration;
        }

        public static DecoratorsConfiguration<T> Sealed<T>(this DecoratorsConfiguration<T> configuration) => configuration.Sealed(out _);

        public static DecoratorsConfiguration<T> Sealed<T>(this DecoratorsConfiguration<T> configuration, out ActionToken removeToken)
        {
            removeToken = configuration.Collection.Components.AddComponent(SealedDecoratorGuard.Instance);
            return configuration;
        }

        public static DecoratorsConfiguration<T> DisposeWith<T, TOwner>(this DecoratorsConfiguration<T> configuration, IComponentOwner<TOwner> owner)
            where TOwner : class, IDisposable
        {
            configuration.Collection.DisposeWith(owner);
            return configuration;
        }

        public static DecoratorsConfiguration<T> DisposeWith<T>(this DecoratorsConfiguration<T> configuration, IHasDisposeCallback owner)
        {
            configuration.Collection.DisposeWith(owner);
            return configuration;
        }

        public static IReadOnlyObservableCollection<T> Collection<T>(this DecoratorsConfiguration<T> configuration) => configuration.CastCollectionTo<T>();

        public static SynchronizedObservableCollection<T> CastCollectionToSynchronized<T>(this DecoratorsConfiguration<T> configuration) =>
            configuration.CastCollectionToSynchronized<T>();

        public static DecoratorsConfiguration<TResult> Select<T, TResult>(this DecoratorsConfiguration<T> configuration, Func<T, TResult?, TResult?> selector,
            Action<T, TResult>? cleanup = null, IEqualityComparer<TResult?>? comparer = null) where T : notnull where TResult : class? =>
            configuration.Select(selector, cleanup, comparer, out _);

        public static DecoratorsConfiguration<TResult> Select<T, TResult>(this DecoratorsConfiguration<T> configuration, Func<T, TResult?, TResult?> selector,
            Action<T, TResult>? cleanup,
            IEqualityComparer<TResult?>? comparer, out ConvertCollectionDecorator<T, TResult> decorator) where T : notnull where TResult : class?
        {
            decorator = new ConvertCollectionDecorator<T, TResult>(configuration.Priority, selector, cleanup, comparer);
            return configuration.Add(decorator).For<TResult>();
        }

        public static DecoratorsConfiguration<TResult> Select<T, TResult>(this DecoratorsConfiguration<T> configuration, Func<T, TResult> selector) where TResult : class?
            => configuration.Select(selector, out _);

        public static DecoratorsConfiguration<TResult> Select<T, TResult>(this DecoratorsConfiguration<T> configuration, Func<T, TResult> selector,
            out ConvertImmutableCollectionDecorator<T, TResult> decorator) where TResult : class?
        {
            decorator = new ConvertImmutableCollectionDecorator<T, TResult>(configuration.Priority, selector);
            return configuration.Add(decorator).For<TResult>();
        }

        public static DecoratorsConfiguration<T> Where<T>(this DecoratorsConfiguration<T> configuration, Func<T, bool> filter, bool nullItemResult = false) =>
            configuration.Where(filter, nullItemResult, out _);

        public static DecoratorsConfiguration<T> Where<T>(this DecoratorsConfiguration<T> configuration, Func<T, bool> filter, bool nullItemResult,
            out FilterCollectionDecorator<T> decorator) =>
            configuration.Where(filter.FilterWhere, nullItemResult, out decorator);

        public static DecoratorsConfiguration<T> Where<T>(this DecoratorsConfiguration<T> configuration, Func<T, int, bool> filter, bool nullItemResult = false) =>
            configuration.Where(filter, nullItemResult, out _);

        public static DecoratorsConfiguration<T> Where<T>(this DecoratorsConfiguration<T> configuration, Func<T, int, bool> filter,
            bool nullItemResult, out FilterCollectionDecorator<T> decorator)
        {
            Should.NotBeNull(filter, nameof(filter));
            decorator = new FilterCollectionDecorator<T>(configuration.Priority, filter) {NullItemResult = nullItemResult};
            return configuration.Add(decorator);
        }

        public static DecoratorsConfiguration<T> Take<T>(this DecoratorsConfiguration<T> configuration, int limit, Func<T, bool>? condition = null) where T : notnull =>
            configuration.Take(limit, condition, out _);

        public static DecoratorsConfiguration<T> Take<T>(this DecoratorsConfiguration<T> configuration, int limit, Func<T, bool>? condition,
            out LimitCollectionDecorator<T> decorator) where T : notnull
        {
            decorator = new LimitCollectionDecorator<T>(configuration.Priority, limit, condition);
            return configuration.Add(decorator);
        }

        public static DecoratorsConfiguration<T> OrderBy<T, TState>(this DecoratorsConfiguration<T> configuration, Func<T, TState?> getState,
            SortingComparerBuilder.BuilderDelegate<TState?> getComparer) => configuration.OrderBy(getState, getComparer, out _);

        public static DecoratorsConfiguration<T> OrderBy<T, TState>(this DecoratorsConfiguration<T> configuration, Func<T, TState?> getState,
            SortingComparerBuilder.BuilderDelegate<TState?> getComparer, out SortCollectionDecorator<TState> decorator)
        {
            Should.NotBeNull(getComparer, nameof(getComparer));
            return configuration.OrderBy(getState, getComparer(SortingComparerBuilder.Get<TState?>()).Build(), out decorator);
        }

        public static DecoratorsConfiguration<T> OrderBy<T, TState>(this DecoratorsConfiguration<T> configuration, Func<T, TState?> getState,
            IComparer<TState?>? comparer = null) => configuration.OrderBy(getState, comparer, out _);

        public static DecoratorsConfiguration<T> OrderBy<T, TState>(this DecoratorsConfiguration<T> configuration, Func<T, TState?> getState, IComparer<TState?>? comparer,
            out SortCollectionDecorator<TState> decorator)
        {
            decorator = new SortCollectionDecorator<TState>(configuration.Priority, getState as Func<object?, TState?> ?? getState.GetStateOrderBy, comparer);
            return configuration.Add(decorator);
        }

        public static DecoratorsConfiguration<T> OrderBy<T>(this DecoratorsConfiguration<T> configuration,
            SortingComparerBuilder.BuilderDelegate<T> getComparer) => configuration.OrderBy(getComparer, out _);

        public static DecoratorsConfiguration<T> OrderBy<T>(this DecoratorsConfiguration<T> configuration,
            SortingComparerBuilder.BuilderDelegate<T> getComparer, out SortCollectionDecorator<object> decorator)
        {
            Should.NotBeNull(getComparer, nameof(getComparer));
            return configuration.OrderBy(getComparer(SortingComparerBuilder.Get<T>()).Build(), out decorator);
        }

        public static DecoratorsConfiguration<T> OrderBy<T>(this DecoratorsConfiguration<T> configuration, IComparer<T> comparer) => configuration.OrderBy(comparer, out _);

        public static DecoratorsConfiguration<T> OrderBy<T>(this DecoratorsConfiguration<T> configuration, IComparer<T> comparer, out SortCollectionDecorator<object> decorator)
        {
            Should.NotBeNull(comparer, nameof(comparer));
            decorator = new SortCollectionDecorator<object>(configuration.Priority, o => o, comparer as IComparer<object?> ?? new WrapperObjectComparer<T>(comparer));
            return configuration.Add(decorator);
        }

        public static DecoratorsConfiguration<TCollection> SelectMany<T, TCollection>(this DecoratorsConfiguration<T> configuration, Func<T, IEnumerable<TCollection>?> selector,
            bool decoratedItems = true)
            where T : class => configuration.SelectMany(selector, decoratedItems, out _).For<TCollection>();

        public static DecoratorsConfiguration<TCollection> SelectMany<T, TCollection>(this DecoratorsConfiguration<T> configuration, Func<T, IEnumerable<TCollection>?> selector,
            bool decoratedItems, out FlattenCollectionDecorator<T> decorator) where T : class
        {
            Should.NotBeNull(selector, nameof(selector));
            return configuration.SelectMany((Func<T, IEnumerable?>) selector, decoratedItems, out decorator).For<TCollection>();
        }

        public static DecoratorsConfiguration<T> SelectMany<T>(this DecoratorsConfiguration<T> configuration, Func<T, IEnumerable?> selector, bool decoratedItems = true)
            where T : class => configuration.SelectMany(selector, decoratedItems, out _);

        public static DecoratorsConfiguration<T> SelectMany<T>(this DecoratorsConfiguration<T> configuration, Func<T, IEnumerable?> selector, bool decoratedItems,
            out FlattenCollectionDecorator<T> decorator) where T : class
        {
            Should.NotBeNull(selector, nameof(selector));
            return configuration.SelectMany(decoratedItems ? selector.SelectManyDecorated : selector.SelectMany, out decorator);
        }

        public static DecoratorsConfiguration<T> SelectMany<T>(this DecoratorsConfiguration<T> configuration, Func<T, FlattenItemInfo> selector) where T : class =>
            configuration.SelectMany(selector, out _);

        public static DecoratorsConfiguration<T> SelectMany<T>(this DecoratorsConfiguration<T> configuration, Func<T, FlattenItemInfo> selector,
            out FlattenCollectionDecorator<T> decorator) where T : class
        {
            decorator = new FlattenCollectionDecorator<T>(configuration.Priority, selector);
            return configuration.Add(decorator);
        }

        public static DecoratorsConfiguration<T> GroupBy<T, TGroup>(this DecoratorsConfiguration<T> configuration, Func<T, TGroup?> getGroup,
            IComparer<TGroup>? comparer = null, IEqualityComparer<TGroup>? equalityComparer = null, bool flatten = true,
            bool flattenDecoratedItems = true)
            where TGroup : class
            where T : notnull
        {
            configuration = configuration.GroupBy(getGroup, equalityComparer);
            if (comparer != null)
                configuration = configuration.For<TGroup>().OrderBy(comparer).For<T>();
            return configuration.FlattenGroup<T, TGroup>(flatten, flattenDecoratedItems);
        }

        public static DecoratorsConfiguration<T> GroupBy<T, TGroup>(this DecoratorsConfiguration<T> configuration, Func<T, TGroup?> getGroup,
            SortingComparerBuilder.BuilderDelegate<TGroup>? getComparer, IEqualityComparer<TGroup>? equalityComparer = null, bool flatten = true,
            bool flattenDecoratedItems = true)
            where TGroup : class
            where T : notnull
        {
            configuration = configuration.GroupBy(getGroup, equalityComparer);
            if (getComparer != null)
                configuration = configuration.For<TGroup>().OrderBy(getComparer).For<T>();
            return configuration.FlattenGroup<T, TGroup>(flatten, flattenDecoratedItems);
        }

        public static DecoratorsConfiguration<T> GroupBy<T, TGroup, TSortState>(this DecoratorsConfiguration<T> configuration, Func<T, TGroup?> getGroup,
            Func<TGroup, TSortState> getSortState, SortingComparerBuilder.BuilderDelegate<TSortState?> getComparer, IEqualityComparer<TGroup>? equalityComparer = null,
            bool flatten = true, bool flattenDecoratedItems = true)
            where TGroup : class
            where T : notnull
        {
            return configuration.GroupBy(getGroup, equalityComparer)
                                .For<TGroup>()
                                .OrderBy(getSortState, getComparer)
                                .For<T>()
                                .FlattenGroup<T, TGroup>(flatten, flattenDecoratedItems);
        }

        public static DecoratorsConfiguration<T> GroupBy<T, TGroup, TSortState>(this DecoratorsConfiguration<T> configuration, Func<T, TGroup?> getGroup,
            Func<TGroup, TSortState> getSortState, IComparer<TSortState?>? comparer = null, IEqualityComparer<TGroup>? equalityComparer = null,
            bool flatten = true, bool flattenDecoratedItems = true)
            where TGroup : class
            where T : notnull
        {
            return configuration.GroupBy(getGroup, equalityComparer)
                                .For<TGroup>()
                                .OrderBy(getSortState, comparer)
                                .For<T>()
                                .FlattenGroup<T, TGroup>(flatten, flattenDecoratedItems);
        }

        public static DecoratorsConfiguration<T> GroupBy<T, TKey>(this DecoratorsConfiguration<T> configuration, Func<T, TKey?> getGroup,
            GroupCollectionDecorator<T, TKey>.UpdateGroupDelegate? updateGroup, IEqualityComparer<TKey>? equalityComparer = null)
            where TKey : class => configuration.GroupBy(getGroup, updateGroup, equalityComparer, out _);

        public static DecoratorsConfiguration<T> GroupBy<T, TKey>(this DecoratorsConfiguration<T> configuration, Func<T, TKey?> getGroup,
            GroupCollectionDecorator<T, TKey>.UpdateGroupDelegate? updateGroup, IEqualityComparer<TKey>? equalityComparer, out GroupCollectionDecorator<T, TKey> decorator)
            where TKey : class
        {
            decorator = new GroupCollectionDecorator<T, TKey>(configuration.Priority, getGroup, updateGroup, equalityComparer);
            return configuration.Add(decorator);
        }

        public static DecoratorsConfiguration<T> Distinct<T, TKey>(this DecoratorsConfiguration<T> configuration, Func<T, TKey> getKey,
            IEqualityComparer<TKey>? equalityComparer = null)
            where TKey : notnull => configuration.Distinct(getKey, equalityComparer, out _);

        public static DecoratorsConfiguration<T> Distinct<T, TKey>(this DecoratorsConfiguration<T> configuration, Func<T, TKey> getKey, IEqualityComparer<TKey>? equalityComparer,
            out DistinctCollectionDecorator<T, TKey> decorator)
            where TKey : notnull
        {
            decorator = new DistinctCollectionDecorator<T, TKey>(configuration.Priority, getKey, equalityComparer);
            return configuration.Add(decorator).UpdatePriority();
        }

        public static DecoratorsConfiguration<T> WithHeaderFooter<T>(this DecoratorsConfiguration<T> configuration, ItemOrIReadOnlyList<object> header,
            ItemOrIReadOnlyList<object> footer = default) => configuration.WithHeaderFooter(header, footer, out _);

        public static DecoratorsConfiguration<T> WithHeaderFooter<T>(this DecoratorsConfiguration<T> configuration, ItemOrIReadOnlyList<object> header,
            ItemOrIReadOnlyList<object> footer, out HeaderFooterCollectionDecorator decorator)
        {
            decorator = new HeaderFooterCollectionDecorator(configuration.Priority) {Header = header, Footer = footer};
            return configuration.Add(decorator);
        }

        public static DecoratorsConfiguration<T> Prepend<T>(this DecoratorsConfiguration<T> configuration, ItemOrIReadOnlyList<object> items) =>
            configuration.WithHeaderFooter(items, default, out _);

        public static DecoratorsConfiguration<T> Append<T>(this DecoratorsConfiguration<T> configuration, ItemOrIReadOnlyList<object> items) =>
            configuration.WithHeaderFooter(default, items, out _);

        public static DecoratorsConfiguration<T> Concat<T>(this DecoratorsConfiguration<T> configuration, IEnumerable items, bool decoratedItemsSource = true)
        {
            Should.NotBeNull(items, nameof(items));
            return configuration.WithHeaderFooter(default, new ItemOrIReadOnlyList<object>(items), out _)
                                .For<IEnumerable>()
                                .SelectMany(decoratedItemsSource ? items.FlattenDecorated : items.Flatten)
                                .For<T>();
        }

        public static DecoratorsConfiguration<T> OfType<T>(this DecoratorsConfiguration<T> configuration) => configuration.OfType(out _);

        public static DecoratorsConfiguration<T> OfType<T>(this DecoratorsConfiguration<T> configuration, out ActionToken removeToken)
        {
            configuration = configuration.For<object?>().Where((t, _) => t is T, false, out var decorator);
            removeToken = ActionToken.FromDelegate((coll, c) => ((IReadOnlyObservableCollection) coll!).RemoveComponent((IComponent<IReadOnlyObservableCollection>) c!),
                configuration.Collection, decorator);
            return configuration;
        }

        public static DecoratorsConfiguration<T> Subscribe<T, TState>(this DecoratorsConfiguration<T> configuration,
            Func<IReadOnlyDictionary<T, (TState state, int count)>, T, TState?, int, bool, TState> onAdded,
            Func<IReadOnlyDictionary<T, (TState state, int count)>, T, TState, int, bool, TState> onRemoved,
            Func<IReadOnlyDictionary<T, (TState state, int count)>, T, TState, int, bool, object?, TState>? onChanged = null,
            Action<IReadOnlyDictionary<T, (TState state, int count)>>? onReset = null,
            IEqualityComparer<T>? comparer = null)
            where T : notnull =>
            configuration.Subscribe(onAdded, onRemoved, onChanged, onReset, comparer, out _);

        public static DecoratorsConfiguration<T> Subscribe<T, TState>(this DecoratorsConfiguration<T> configuration,
            Func<IReadOnlyDictionary<T, (TState state, int count)>, T, TState?, int, bool, TState> onAdded,
            Func<IReadOnlyDictionary<T, (TState state, int count)>, T, TState, int, bool, TState> onRemoved,
            Func<IReadOnlyDictionary<T, (TState state, int count)>, T, TState, int, bool, object?, TState>? onChanged,
            Action<IReadOnlyDictionary<T, (TState state, int count)>>? onReset,
            IEqualityComparer<T>? comparer, out ActionToken removeToken)
            where T : notnull
        {
            var decorator = new TrackerCollectionDecorator<T, TState>(configuration.Priority, onAdded, onRemoved, onChanged, onReset, comparer);
            return configuration.Add(decorator, null, out removeToken);
        }

        public static DecoratorsConfiguration<T> Bind<T>(this DecoratorsConfiguration<T> configuration, out IReadOnlyObservableCollection<T> collection,
            bool disposeSourceOnDispose = false, bool isWeak = true, bool materialize = false)
        {
            collection = new DecoratedReadOnlyObservableCollection<T>(configuration.Collection, configuration.Priority, disposeSourceOnDispose, isWeak, materialize);
            return configuration.UpdatePriority();
        }

        public static IReadOnlyObservableCollection<T> Bind<T>(this DecoratorsConfiguration<T> configuration, bool disposeSourceOnDispose = false, bool isWeak = true,
            bool materialize = false)
        {
            configuration.Bind<T>(out var collection, disposeSourceOnDispose, isWeak, materialize);
            return collection;
        }

        public static DecoratorsConfiguration<T> TrackSelectedItem<T>(this DecoratorsConfiguration<T> configuration, Func<T?> getSelectedItem, Action<T?> setSelectedItem,
            Func<IEnumerable<T>, T?, T?>? getDefault = null, IEqualityComparer<T>? comparer = null) where T : class =>
            configuration.TrackSelectedItem(getSelectedItem, setSelectedItem, getDefault, comparer, out _);

        public static DecoratorsConfiguration<T> TrackSelectedItem<T>(this DecoratorsConfiguration<T> configuration, Func<T?> getSelectedItem, Action<T?> setSelectedItem,
            Func<IEnumerable<T>, T?, T?>? getDefault, IEqualityComparer<T>? comparer, out ActionToken removeToken) where T : class
        {
            Should.NotBeNull(getSelectedItem, nameof(getSelectedItem));
            Should.NotBeNull(setSelectedItem, nameof(setSelectedItem));
            return configuration.Subscribe<T, object?>((items, item, _, count, isReset) =>
            {
                if (!isReset)
                {
                    var selectedItem = getSelectedItem();
                    if (selectedItem == null || !(comparer ?? EqualityComparer<T>.Default).Equals(selectedItem, item) && !items.ContainsKey(selectedItem))
                        SetSelectedItem(setSelectedItem, getDefault, items, selectedItem, item, count);
                }

                return null;
            }, (items, _, _, _, isReset) =>
            {
                if (!isReset)
                {
                    var selectedItem = getSelectedItem();
                    if (selectedItem == null || !items.ContainsKey(selectedItem))
                        SetSelectedItem(setSelectedItem, getDefault, items, selectedItem, null, -1);
                }

                return null;
            }, null, items =>
            {
                var selectedItem = getSelectedItem();
                if (selectedItem == null || !items.ContainsKey(selectedItem))
                    SetSelectedItem(setSelectedItem, getDefault, items, selectedItem, null, -1);
            }, comparer, out removeToken);
        }

        public static DecoratorsConfiguration<T> AutoRefreshOnPropertyChanged<T>(this DecoratorsConfiguration<T> configuration, ItemOrArray<string> members, object? args = null)
            where T : class => configuration.AutoRefreshOnPropertyChanged<T>(members, args, out _);

        public static DecoratorsConfiguration<T> AutoRefreshOnPropertyChanged<T>(this DecoratorsConfiguration<T> configuration, ItemOrArray<string> members,
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

        public static DecoratorsConfiguration<T> AutoRefreshOnObservable<T, TSignal>(this DecoratorsConfiguration<T> configuration, Func<T, IObservable<TSignal>> getObservable,
            object? args = null) where T : class => configuration.AutoRefreshOnObservable(getObservable, args, out _);

        public static DecoratorsConfiguration<T> AutoRefreshOnObservable<T, TSignal>(this DecoratorsConfiguration<T> configuration, Func<T, IObservable<TSignal>> getObservable,
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

        public static DecoratorsConfiguration<T> Min<T, TResult>(this DecoratorsConfiguration<T> configuration, Func<T, TResult> selector, Action<TResult?> onChanged,
            TResult? defaultValue = default, Func<T, bool>? predicate = null, IComparer<TResult?>? comparer = null)
            where T : notnull =>
            configuration.MaxMin(selector, onChanged, defaultValue, predicate, comparer, false, out _);

        public static DecoratorsConfiguration<T> Min<T, TResult>(this DecoratorsConfiguration<T> configuration, Func<T, TResult> selector, Action<TResult?> onChanged,
            TResult? defaultValue, Func<T, bool>? predicate, IComparer<TResult?>? comparer, out ActionToken removeToken)
            where T : notnull =>
            configuration.MaxMin(selector, onChanged, defaultValue, predicate, comparer, false, out removeToken);

        public static DecoratorsConfiguration<T> Max<T, TResult>(this DecoratorsConfiguration<T> configuration, Func<T, TResult> selector, Action<TResult?> onChanged,
            TResult? defaultValue = default, Func<T, bool>? predicate = null, IComparer<TResult?>? comparer = null)
            where T : notnull =>
            configuration.MaxMin(selector, onChanged, defaultValue, predicate, comparer, true, out _);

        public static DecoratorsConfiguration<T> Max<T, TResult>(this DecoratorsConfiguration<T> configuration, Func<T, TResult> selector, Action<TResult?> onChanged,
            TResult? defaultValue, Func<T, bool>? predicate, IComparer<TResult?>? comparer, out ActionToken removeToken)
            where T : notnull =>
            configuration.MaxMin(selector, onChanged, defaultValue, predicate, comparer, true, out removeToken);

        public static DecoratorsConfiguration<T> Count<T>(this DecoratorsConfiguration<T> configuration, Action<int> onChanged, Func<T, bool>? predicate = null)
            where T : notnull => configuration.Count(onChanged, predicate, out _);

        public static DecoratorsConfiguration<T> Count<T>(this DecoratorsConfiguration<T> configuration, Action<int> onChanged, Func<T, bool>? predicate,
            out ActionToken removeToken)
            where T : notnull =>
            configuration.Accumulate(0, _ => 1, (x1, x2) => x1 + x2, (x1, x2) => x1 - x2, onChanged, predicate, out removeToken);

        public static DecoratorsConfiguration<T> Sum<T>(this DecoratorsConfiguration<T> configuration, Func<T, int> selector, Action<int> onChanged,
            Func<T, bool>? predicate = null)
            where T : notnull => configuration.Sum(selector, onChanged, predicate, out _);

        public static DecoratorsConfiguration<T> Sum<T>(this DecoratorsConfiguration<T> configuration, Func<T, int?> selector, Action<int?> onChanged,
            Func<T, bool>? predicate = null)
            where T : notnull => configuration.Sum(selector, onChanged, predicate, out _);

        public static DecoratorsConfiguration<T> Sum<T>(this DecoratorsConfiguration<T> configuration, Func<T, long> selector, Action<long> onChanged,
            Func<T, bool>? predicate = null)
            where T : notnull => configuration.Sum(selector, onChanged, predicate, out _);

        public static DecoratorsConfiguration<T> Sum<T>(this DecoratorsConfiguration<T> configuration, Func<T, long?> selector, Action<long?> onChanged,
            Func<T, bool>? predicate = null)
            where T : notnull => configuration.Sum(selector, onChanged, predicate, out _);

        public static DecoratorsConfiguration<T> Sum<T>(this DecoratorsConfiguration<T> configuration, Func<T, float> selector, Action<float> onChanged,
            Func<T, bool>? predicate = null)
            where T : notnull => configuration.Sum(selector, onChanged, predicate, out _);

        public static DecoratorsConfiguration<T> Sum<T>(this DecoratorsConfiguration<T> configuration, Func<T, float?> selector, Action<float?> onChanged,
            Func<T, bool>? predicate = null)
            where T : notnull => configuration.Sum(selector, onChanged, predicate, out _);

        public static DecoratorsConfiguration<T> Sum<T>(this DecoratorsConfiguration<T> configuration, Func<T, double> selector, Action<double> onChanged,
            Func<T, bool>? predicate = null)
            where T : notnull => configuration.Sum(selector, onChanged, predicate, out _);

        public static DecoratorsConfiguration<T> Sum<T>(this DecoratorsConfiguration<T> configuration, Func<T, double?> selector, Action<double?> onChanged,
            Func<T, bool>? predicate = null)
            where T : notnull => configuration.Sum(selector, onChanged, predicate, out _);

        public static DecoratorsConfiguration<T> Sum<T>(this DecoratorsConfiguration<T> configuration, Func<T, decimal> selector, Action<decimal> onChanged,
            Func<T, bool>? predicate = null)
            where T : notnull => configuration.Sum(selector, onChanged, predicate, out _);

        public static DecoratorsConfiguration<T> Sum<T>(this DecoratorsConfiguration<T> configuration, Func<T, decimal?> selector, Action<decimal?> onChanged,
            Func<T, bool>? predicate = null)
            where T : notnull => configuration.Sum(selector, onChanged, predicate, out _);

        public static DecoratorsConfiguration<T> Sum<T>(this DecoratorsConfiguration<T> configuration, Func<T, int> selector, Action<int> onChanged, Func<T, bool>? predicate,
            out ActionToken removeToken)
            where T : notnull =>
            configuration.Accumulate(0, selector, (x1, x2) => x1 + x2, (x1, x2) => x1 - x2, onChanged, predicate, out removeToken);

        public static DecoratorsConfiguration<T> Sum<T>(this DecoratorsConfiguration<T> configuration, Func<T, int?> selector, Action<int?> onChanged, Func<T, bool>? predicate,
            out ActionToken removeToken)
            where T : notnull =>
            configuration.Accumulate(0, selector, (x1, x2) => x1 + x2, (x1, x2) => x1 - x2, onChanged, predicate, out removeToken);

        public static DecoratorsConfiguration<T> Sum<T>(this DecoratorsConfiguration<T> configuration, Func<T, long> selector, Action<long> onChanged, Func<T, bool>? predicate,
            out ActionToken removeToken)
            where T : notnull =>
            configuration.Accumulate(0, selector, (x1, x2) => x1 + x2, (x1, x2) => x1 - x2, onChanged, predicate, out removeToken);

        public static DecoratorsConfiguration<T> Sum<T>(this DecoratorsConfiguration<T> configuration, Func<T, long?> selector, Action<long?> onChanged, Func<T, bool>? predicate,
            out ActionToken removeToken)
            where T : notnull =>
            configuration.Accumulate(0, selector, (x1, x2) => x1 + x2, (x1, x2) => x1 - x2, onChanged, predicate, out removeToken);

        public static DecoratorsConfiguration<T> Sum<T>(this DecoratorsConfiguration<T> configuration, Func<T, float> selector, Action<float> onChanged, Func<T, bool>? predicate,
            out ActionToken removeToken)
            where T : notnull =>
            configuration.Accumulate(0, selector, (x1, x2) => x1 + x2, (x1, x2) => x1 - x2, onChanged, predicate, out removeToken);

        public static DecoratorsConfiguration<T> Sum<T>(this DecoratorsConfiguration<T> configuration, Func<T, float?> selector, Action<float?> onChanged,
            Func<T, bool>? predicate, out ActionToken removeToken)
            where T : notnull =>
            configuration.Accumulate(0, selector, (x1, x2) => x1 + x2, (x1, x2) => x1 - x2, onChanged, predicate, out removeToken);

        public static DecoratorsConfiguration<T> Sum<T>(this DecoratorsConfiguration<T> configuration, Func<T, double> selector, Action<double> onChanged,
            Func<T, bool>? predicate, out ActionToken removeToken)
            where T : notnull =>
            configuration.Accumulate(0, selector, (x1, x2) => x1 + x2, (x1, x2) => x1 - x2, onChanged, predicate, out removeToken);

        public static DecoratorsConfiguration<T> Sum<T>(this DecoratorsConfiguration<T> configuration, Func<T, double?> selector, Action<double?> onChanged,
            Func<T, bool>? predicate, out ActionToken removeToken)
            where T : notnull =>
            configuration.Accumulate(0, selector, (x1, x2) => x1 + x2, (x1, x2) => x1 - x2, onChanged, predicate, out removeToken);

        public static DecoratorsConfiguration<T> Sum<T>(this DecoratorsConfiguration<T> configuration, Func<T, decimal> selector, Action<decimal> onChanged,
            Func<T, bool>? predicate, out ActionToken removeToken)
            where T : notnull =>
            configuration.Accumulate(0, selector, (x1, x2) => x1 + x2, (x1, x2) => x1 - x2, onChanged, predicate, out removeToken);

        public static DecoratorsConfiguration<T> Sum<T>(this DecoratorsConfiguration<T> configuration, Func<T, decimal?> selector, Action<decimal?> onChanged,
            Func<T, bool>? predicate, out ActionToken removeToken)
            where T : notnull =>
            configuration.Accumulate(0, selector, (x1, x2) => x1 + x2, (x1, x2) => x1 - x2, onChanged, predicate, out removeToken);

        public static DecoratorsConfiguration<T> All<T>(this DecoratorsConfiguration<T> configuration, Func<T, bool> selector, Action<bool> onChanged,
            Func<T, bool>? predicate = null)
            where T : notnull => configuration.All(selector, onChanged, predicate, out _);

        public static DecoratorsConfiguration<T> All<T>(this DecoratorsConfiguration<T> configuration, Func<T, bool> selector, Action<bool> onChanged, Func<T, bool>? predicate,
            out ActionToken removeToken)
            where T : notnull
        {
            Should.NotBeNull(selector, nameof(selector));
            Should.NotBeNull(onChanged, nameof(onChanged));
            bool lastValue = false;
            return configuration.Accumulate(default, selector.AllSelector, (x1, x2) => (x1.total + x2.total, x1.count + x2.count),
                (x1, x2) => (x1.total - x2.total, x1.count - x2.count), count =>
                {
                    var value = count.total == count.count;
                    if (lastValue == value)
                        return;
                    lastValue = value;
                    onChanged(value);
                }, predicate, out removeToken);
        }

        public static DecoratorsConfiguration<T> Any<T>(this DecoratorsConfiguration<T> configuration, Func<T, bool> selector, Action<bool> onChanged,
            Func<T, bool>? predicate = null)
            where T : notnull => configuration.Any(selector, onChanged, predicate, out _);

        public static DecoratorsConfiguration<T> Any<T>(this DecoratorsConfiguration<T> configuration, Func<T, bool> selector, Action<bool> onChanged, Func<T, bool>? predicate,
            out ActionToken removeToken)
            where T : notnull
        {
            Should.NotBeNull(selector, nameof(selector));
            Should.NotBeNull(onChanged, nameof(onChanged));
            bool lastValue = false;
            return configuration.Accumulate(0, selector.AnySelector, (x1, x2) => x1 + x2, (x1, x2) => x1 - x2, count =>
            {
                var value = count != 0;
                if (lastValue == value)
                    return;
                lastValue = value;
                onChanged(value);
            }, predicate, out removeToken);
        }

        public static DecoratorsConfiguration<T> Accumulate<T, TResult>(this DecoratorsConfiguration<T> configuration, TResult seed, Func<T, TResult> selector,
            Func<TResult, TResult, TResult> add, Func<TResult, TResult, TResult> remove, Action<TResult> onChanged, Func<T, bool>? predicate = null)
            where T : notnull =>
            configuration.Accumulate(seed, selector, add, remove, onChanged, predicate, out _);

        public static DecoratorsConfiguration<T> Accumulate<T, TResult>(this DecoratorsConfiguration<T> configuration, TResult seed, Func<T, TResult> selector,
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

        public static DecoratorsConfiguration<T> FirstOrDefault<T>(this DecoratorsConfiguration<T> configuration, Action<T?> setter, Func<T, bool>? predicate = null,
            bool useCurrentCollection = false) => configuration.FirstOrLast(setter, predicate, true, useCurrentCollection, out _);

        public static DecoratorsConfiguration<T> FirstOrDefault<T>(this DecoratorsConfiguration<T> configuration, Action<T?> setter, Func<T, bool>? predicate,
            bool useCurrentCollection, out ActionToken removeToken) => configuration.FirstOrLast(setter, predicate, true, useCurrentCollection, out removeToken);

        public static DecoratorsConfiguration<T> LastOrDefault<T>(this DecoratorsConfiguration<T> configuration, Action<T?> setter, Func<T, bool>? predicate = null,
            bool useCurrentCollection = false) => configuration.FirstOrLast(setter, predicate, false, useCurrentCollection, out _);

        public static DecoratorsConfiguration<T> LastOrDefault<T>(this DecoratorsConfiguration<T> configuration, Action<T?> setter, Func<T, bool>? predicate,
            bool useCurrentCollection, out ActionToken removeToken) => configuration.FirstOrLast(setter, predicate, false, useCurrentCollection, out removeToken);

        private static DecoratorsConfiguration<T> FirstOrLast<T>(this DecoratorsConfiguration<T> configuration, Action<T?> setter, Func<T, bool>? predicate, bool isFirst,
            bool useCurrentCollection, out ActionToken removeToken)
        {
            if (typeof(T) == typeof(object) && predicate == null)
            {
                var decorator = new FirstLastTrackerCollectionDecorator(configuration.Priority, (Action<object?>) (object) setter, isFirst);
                return configuration.Add(decorator, null, out removeToken);
            }

            if (useCurrentCollection)
            {
                configuration = configuration.For<object?>().Where(predicate == null ? o => o is T : predicate.FirstLastPredicate, false, out var filter);
                var tracker = new FirstLastTrackerCollectionDecorator(configuration.Priority, setter.FirstLastSetter, isFirst);
                removeToken = ActionToken.FromHandler(tracker, configuration.Collection, filter);
                return configuration.Add(tracker);
            }

            var childConfig = configuration.For<object?>()
                                           .Bind(false, false)
                                           .ConfigureDecorators(0)
                                           .Where(predicate == null ? o => o is T : predicate.FirstLastPredicate);
            childConfig.Add(new FirstLastTrackerCollectionDecorator(childConfig.Priority, setter.FirstLastSetter, isFirst));
            removeToken = ActionToken.FromDisposable(childConfig.Collection);
            return configuration;
        }

        public static void ApplyChangesTo<T>(this CollectionGroupChangedAction action, IList<T> items, IEnumerable<T> groupItems, T? item, object? args,
            bool checkDisposable = true)
        {
            switch (action)
            {
                case CollectionGroupChangedAction.Clear:
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

        public static void Move<T>(this IList<T> list, int oldIndex, int newIndex)
        {
            Should.NotBeNull(list, nameof(list));
            if (list is ObservableCollection<T> observableCollection)
            {
                observableCollection.Move(oldIndex, newIndex);
                return;
            }

#if NET5_0
            if (list is List<T> l)
            {
                var old = list[oldIndex];
                var span = CollectionsMarshal.AsSpan(l);
                if (newIndex < oldIndex)
                    Copy(span, newIndex, newIndex + 1, oldIndex - newIndex);
                else
                    Copy(span, oldIndex + 1, oldIndex, newIndex - oldIndex);
                list[newIndex] = old;
                return;
            }
#endif
            var tmp = list[oldIndex];
            list.RemoveAt(oldIndex);
            list.Insert(newIndex, tmp);
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

        internal static bool IsNullOrEmpty<T>([NoEnumeration] [NotNullWhen(false)] this IEnumerable<T>? enumerable)
        {
            if (enumerable.TryGetCount(out var count))
                return count == 0;
            return false;
        }

        internal static int CountEx<T>(this IEnumerable<T>? enumerable) => enumerable.TryGetCount(out var count) ? count : enumerable.Count();

        internal static T? FirstOrDefaultEx<T>(this IEnumerable<T> items)
        {
            if (items is IReadOnlyList<T> list)
            {
                if (list.Count == 0)
                    return default;
                return list[0];
            }

            return Enumerable.FirstOrDefault(items);
        }

        internal static T? LastOrDefaultEx<T>(this IEnumerable<T> items)
        {
            if (items is IReadOnlyList<T> list)
            {
                if (list.Count == 0)
                    return default;
                return list[list.Count - 1];
            }

            return items.LastOrDefault();
        }

        internal static bool TryGetCount<T>([NoEnumeration] [NotNullWhen(false)] this IEnumerable<T>? enumerable, out int count)
        {
            if (enumerable == null)
            {
                count = 0;
                return true;
            }

            if (enumerable is IReadOnlyCollection<T> c)
            {
                count = c.Count;
                return true;
            }

            if (enumerable is ICollection<T> coll)
            {
                count = coll.Count;
                return true;
            }

            count = -1;
            return false;
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

        internal static KeyValuePair<TKey, TValue> FirstOrDefault<TKey, TValue>(this Dictionary<TKey, TValue>? dictionary, KeyValuePair<TKey, TValue> defaultValue = default)
            where TKey : notnull
        {
            if (dictionary == null)
                return defaultValue;
            foreach (var value in dictionary)
                return value;
            return defaultValue;
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

        private static DecoratorsConfiguration<T> GroupBy<T, TGroup>(this DecoratorsConfiguration<T> configuration, Func<T, TGroup?> getGroup,
            IEqualityComparer<TGroup>? equalityComparer)
            where TGroup : class
            where T : notnull
        {
            return configuration.GroupBy(getGroup, (group, items, action, item, args) =>
                                {
                                    if (group is ICollectionGroup<T> g && (action != CollectionGroupChangedAction.Clear || !g.TryCleanup()))
                                        action.ApplyChangesTo(g.Items, items, item, args);
                                }, equalityComparer)
                                .Where(_ => false);
        }

        private static DecoratorsConfiguration<T> FlattenGroup<T, TGroup>(this DecoratorsConfiguration<T> configuration, bool flatten, bool flattenDecoratedItems)
            where TGroup : class
        {
            if (!flatten)
                return configuration;

            return configuration.For<TGroup>()
                                .SelectMany(flattenDecoratedItems
                                    ? group => new FlattenItemInfo((group as ICollectionGroup<T>)?.Items, true)
                                    : group => new FlattenItemInfo((group as ICollectionGroup<T>)?.Items, false))
                                .For<T>();
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

        private static DecoratorsConfiguration<T> MaxMin<T, TResult>(this DecoratorsConfiguration<T> configuration, Func<T, TResult> selector, Action<TResult?> onChanged,
            TResult? defaultValue, Func<T, bool>? predicate, IComparer<TResult?>? comparer, bool isMax, out ActionToken removeToken)
            where T : notnull
        {
            Should.NotBeNull(selector, nameof(selector));
            Should.NotBeNull(onChanged, nameof(onChanged));
            var closure = new MaxMinClosure<T, TResult>(defaultValue, selector, onChanged, comparer, isMax, predicate);
            return configuration.Subscribe<T, (TResult?, bool)>(closure.OnAdded, closure.OnRemoved, closure.OnChanged, closure.OnReset, null, out removeToken);
        }

        private static (int total, int count) AllSelector<T>(this Func<T, bool> selector, T item) => (1, selector(item) ? 1 : 0);

        private static int AnySelector<T>(this Func<T, bool> selector, T item) => selector(item) ? 1 : 0;

        private static void SetSelectedItem<T>(Action<T?> setSelectedItem, Func<IEnumerable<T>, T?, T?>? getDefault, IReadOnlyDictionary<T, (object? state, int count)> items,
            T? selectedItem, T? newItem, int count) where T : class
        {
            var item = getDefault == null
                ? ((Dictionary<T, (object? state, int count)>) items).FirstOrDefault(new KeyValuePair<T, (object? state, int count)>(newItem!, (null, 0))).Key
                : getDefault(count == 1 ? items.Keys.Prepend(newItem!) : items.Keys, selectedItem);
            if (!ReferenceEquals(selectedItem, item))
                setSelectedItem(item);
        }

        private static void FirstLastSetter<T>(this Action<T?> setter, object? item)
        {
            if (item == null)
                setter(default);
            else
                setter((T?) item);
        }

        private static bool FirstLastPredicate<T>(this Func<T, bool> predicate, object? item)
        {
            if (item is T tItem)
                return predicate(tItem);
            return false;
        }

        private static TState? GetStateOrderBy<T, TState>(this Func<T, TState?> getState, object? item)
        {
            if (item is T itemT)
                return getState(itemT);
            return default;
        }

        private static bool FilterWhere<T>(this Func<T, bool> filter, T item, int _) => filter(item);

#if NET461
        private static IEnumerable<T> Prepend<T>(this IEnumerable<T> values, T value)
        {
            yield return value;
            foreach (T item in values)
                yield return item;
        }
#endif

#if SPAN_API
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Copy<T>(Span<T> source, int sourceIndex, int destinationIndex, int length) =>
            source.Slice(sourceIndex, length).CopyTo(source.Slice(destinationIndex));
#endif

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