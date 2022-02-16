using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
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
using MugenMvvm.Delegates;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;
using MugenMvvm.Models;

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
        {
            Should.NotBeNull(items, nameof(items));
            if (items is IReadOnlyObservableCollection collection)
                return collection.Bind(disposeSourceOnDispose, isWeak, materialize);

            if (!TypeChecker.IsValueType<T>())
                return (IReadOnlyObservableCollection<object?>) (object) new ObservableList<T>(items);

            var result = new ObservableList<object?>(items.TryGetCount(out var count) ? count : 0);
            foreach (var item in items)
                result.Add(item);
            return result;
        }

        public static IReadOnlyObservableCollection<T> CreateDerivedCollectionSource<T>(this IEnumerable<T> items, bool disposeSourceOnDispose = false, bool isWeak = true)
        {
            Should.NotBeNull(items, nameof(items));
            if (items is IReadOnlyObservableCollection<T> collection)
                return collection.BindToSource(disposeSourceOnDispose, isWeak);
            return new ObservableList<T>(items);
        }

        public static DecoratorsConfiguration<T> ConfigureDecorators<T>(this IReadOnlyObservableCollection<T> genericCollection, bool allowNull = false,
            int? priority = null, int step = 10) => ConfigureDecorators<T>(collection: genericCollection, allowNull, priority, step);

        public static DecoratorsConfiguration<object?> ConfigureDecorators(this IReadOnlyObservableCollection collection, bool allowNull = false, int? priority = null,
            int step = 10) => collection.ConfigureDecorators<object?>(allowNull, priority, step);

        public static DecoratorsConfiguration<T> ConfigureDecorators<T>(this IReadOnlyObservableCollection collection, bool allowNull = false, int? priority = null, int step = 10)
        {
            Should.NotBeNull(collection, nameof(collection));
            if (priority == null)
            {
                var array = collection.GetComponents<ICollectionDecorator>();
                priority = array.Count == 0 ? 0 : GetComponentPriority(array[array.Count - 1]) - step;
            }

            return new DecoratorsConfiguration<T>(collection, priority.Value, step, allowNull);
        }

        [return: NotNullIfNotNull("collection")]
        public static IEnumerable<object?>? DecoratedItems(this IReadOnlyObservableCollection? collection)
        {
            if (collection == null)
                return null;
            var component = collection.GetComponentOptional<ICollectionDecoratorManagerComponent>();
            return component == null ? collection.AsEnumerable() : component.Decorate(collection, null, false);
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

        public static void InvalidateDecorators(this IReadOnlyObservableCollection collection, ICollectionDecorator? decorator = null)
        {
            Should.NotBeNull(collection, nameof(collection));
            collection.GetComponentOptional<ICollectionDecoratorManagerComponent>()?.Invalidate(collection, decorator);
        }

        public static void OnReset(this ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ICollectionDecorator decorator)
        {
            Should.NotBeNull(decoratorManager, nameof(decoratorManager));
            Should.NotBeNull(collection, nameof(collection));
            Should.NotBeNull(decorator, nameof(decorator));
            decoratorManager.OnReset(collection, decorator, decoratorManager.Decorate(collection, decorator, false));
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
            configuration.Collection.AddComponent(SealedDecoratorGuard.Instance);
            configuration.Collection.Components.AddComponent(SealedDecoratorGuard.Instance);
            removeToken = ActionToken.FromHandler(SealedDecoratorGuard.Instance, configuration.Collection);
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

        public static DecoratorsConfiguration<T> RegisterDisposeToken<T>(this DecoratorsConfiguration<T> configuration, IDisposable token)
        {
            configuration.Collection.RegisterDisposeToken(token);
            return configuration;
        }

        public static DecoratorsConfiguration<T> RegisterDisposeToken<T>(this DecoratorsConfiguration<T> configuration, ActionToken token)
        {
            configuration.Collection.RegisterDisposeToken(token);
            return configuration;
        }

        public static IReadOnlyObservableCollection<T> Collection<T>(this DecoratorsConfiguration<T> configuration) => configuration.CastCollectionTo<T>();

        public static ObservableList<T> CastToList<T>(this DecoratorsConfiguration<T> configuration) => configuration.CastToList<T>();

#pragma warning disable CS8714
        public static ObservableSet<T> CastToSet<T>(this DecoratorsConfiguration<T> configuration) => configuration.CastToSet<T>();
#pragma warning restore CS8714

        public static DecoratorsConfiguration<TResult> Select<T, TResult>(this DecoratorsConfiguration<T> configuration, Func<T, TResult?, TResult> selector,
            Action<T, TResult>? cleanup = null, IEqualityComparer<T>? comparerFrom = null, IEqualityComparer<TResult?>? comparerTo = null) where TResult : class? =>
            configuration.Select(new Func<T, TResult?, Optional<TResult>>(selector.OptionalClosure), cleanup, comparerFrom, comparerTo, out _);

        public static DecoratorsConfiguration<TResult> Select<T, TResult>(this DecoratorsConfiguration<T> configuration, Func<T, TResult?, TResult> selector,
            Action<T, TResult>? cleanup, IEqualityComparer<T>? comparerFrom, IEqualityComparer<TResult?>? comparerTo, out ConvertCollectionDecorator<T, TResult> decorator)
            where TResult : class? =>
            configuration.Select(new Func<T, TResult?, Optional<TResult>>(selector.OptionalClosure), cleanup, comparerFrom, comparerTo, out decorator);

        public static DecoratorsConfiguration<TResult> Select<T, TResult>(this DecoratorsConfiguration<T> configuration, Func<T, TResult?, Optional<TResult>> selector,
            Action<T, TResult>? cleanup = null, IEqualityComparer<T>? comparerFrom = null, IEqualityComparer<TResult?>? comparerTo = null)
            where TResult : class? => configuration.Select(selector, cleanup, comparerFrom, comparerTo, out _);

        public static DecoratorsConfiguration<TResult> Select<T, TResult>(this DecoratorsConfiguration<T> configuration, Func<T, TResult?, Optional<TResult>> selector,
            Action<T, TResult>? cleanup, IEqualityComparer<T>? comparerFrom, IEqualityComparer<TResult?>? comparerTo, out ConvertCollectionDecorator<T, TResult> decorator)
            where TResult : class?
        {
            decorator = new ConvertCollectionDecorator<T, TResult>(configuration.Priority, configuration.AllowNull, selector, cleanup, comparerFrom, comparerTo);
            return configuration.Add(decorator).For<TResult>();
        }

        public static DecoratorsConfiguration<TResult> Select<T, TResult>(this DecoratorsConfiguration<T> configuration, Func<T, TResult> selector) where TResult : class?
            => configuration.Select(selector, out _);

        public static DecoratorsConfiguration<TResult> Select<T, TResult>(this DecoratorsConfiguration<T> configuration, Func<T, TResult> selector,
            out ConvertImmutableCollectionDecorator<T, TResult> decorator) where TResult : class?
        {
            decorator = new ConvertImmutableCollectionDecorator<T, TResult>(configuration.Priority, configuration.AllowNull, selector);
            return configuration.Add(decorator).For<TResult>();
        }

        public static DecoratorsConfiguration<T> Where<T>(this DecoratorsConfiguration<T> configuration, Func<T, bool> filter) =>
            configuration.Where(filter, out _);

        public static DecoratorsConfiguration<T> Where<T>(this DecoratorsConfiguration<T> configuration, Func<T, bool> filter, out FilterCollectionDecorator<T> decorator) =>
            configuration.Where(filter.FilterWhere, out decorator);

        public static DecoratorsConfiguration<T> Where<T>(this DecoratorsConfiguration<T> configuration, Func<T, int, bool> filter) =>
            configuration.Where(filter, out _);

        public static DecoratorsConfiguration<T> Where<T>(this DecoratorsConfiguration<T> configuration, Func<T, int, bool> filter, out FilterCollectionDecorator<T> decorator)
        {
            Should.NotBeNull(filter, nameof(filter));
            decorator = new FilterCollectionDecorator<T>(configuration.Priority, configuration.AllowNull, filter);
            return configuration.Add(decorator);
        }

        public static DecoratorsConfiguration<T> Take<T>(this DecoratorsConfiguration<T> configuration, int limit, Func<T, bool>? condition = null) =>
            configuration.Take(limit, condition, out _);

        public static DecoratorsConfiguration<T> Take<T>(this DecoratorsConfiguration<T> configuration, int limit, Func<T, bool>? condition,
            out LimitCollectionDecorator<T> decorator)
        {
            decorator = new LimitCollectionDecorator<T>(configuration.Priority, configuration.AllowNull, limit, condition);
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
            bool decoratedItems = true, Action<T, IEnumerable?>? cleanup = null, bool recycle = false)
            where T : class? => configuration.SelectMany(selector, decoratedItems, cleanup, recycle, out _);

        public static DecoratorsConfiguration<TCollection> SelectMany<T, TCollection>(this DecoratorsConfiguration<T> configuration, Func<T, IEnumerable<TCollection>?> selector,
            bool decoratedItems, Action<T, IEnumerable?>? cleanup, bool recycle, out FlattenCollectionDecorator<T> decorator) where T : class?
        {
            Should.NotBeNull(selector, nameof(selector));
            return configuration.SelectMany((Func<T, IEnumerable?>) selector, decoratedItems, cleanup, recycle, out decorator).For<TCollection>();
        }

        public static DecoratorsConfiguration<T> SelectMany<T>(this DecoratorsConfiguration<T> configuration, Func<T, IEnumerable?> selector, bool decoratedItems = true,
            Action<T, IEnumerable?>? cleanup = null, bool recycle = false)
            where T : class? => configuration.SelectMany(selector, decoratedItems, cleanup, recycle, out _);

        public static DecoratorsConfiguration<T> SelectMany<T>(this DecoratorsConfiguration<T> configuration, Func<T, IEnumerable?> selector, bool decoratedItems,
            Action<T, IEnumerable?>? cleanup, bool recycle, out FlattenCollectionDecorator<T> decorator) where T : class?
        {
            Should.NotBeNull(selector, nameof(selector));
            if (decoratedItems)
                return configuration.SelectMany(recycle ? selector.SelectManyDecoratedRecycle : selector.SelectManyDecorated, cleanup, out decorator);
            return configuration.SelectMany(recycle ? selector.SelectManyRecycle : selector.SelectMany, cleanup, out decorator);
        }

        public static DecoratorsConfiguration<T> SelectMany<T>(this DecoratorsConfiguration<T> configuration, Func<T, FlattenItemInfo, FlattenItemInfo> selector,
            Action<T, IEnumerable?>? cleanup = null) where T : class? =>
            configuration.SelectMany(selector, cleanup, out _);

        public static DecoratorsConfiguration<T> SelectMany<T>(this DecoratorsConfiguration<T> configuration, Func<T, FlattenItemInfo, FlattenItemInfo> selector,
            Action<T, IEnumerable?>? cleanup, out FlattenCollectionDecorator<T> decorator) where T : class?
        {
            decorator = new FlattenCollectionDecorator<T>(configuration.Priority, configuration.AllowNull, selector, cleanup);
            return configuration.Add(decorator);
        }

        public static DecoratorsConfiguration<T> Distinct<T, TKey>(this DecoratorsConfiguration<T> configuration, Func<T, TKey> getKey,
            IEqualityComparer<TKey>? equalityComparer = null)
            where TKey : notnull => configuration.Distinct(getKey.OptionalClosure, equalityComparer, out _);

        public static DecoratorsConfiguration<T> Distinct<T, TKey>(this DecoratorsConfiguration<T> configuration, Func<T, TKey> getKey,
            IEqualityComparer<TKey>? equalityComparer, out DistinctCollectionDecorator<T, TKey> decorator)
            where TKey : notnull =>
            configuration.Distinct(getKey.OptionalClosure, equalityComparer, out decorator);

        public static DecoratorsConfiguration<T> Distinct<T, TKey>(this DecoratorsConfiguration<T> configuration, Func<T, Optional<TKey>> getKey,
            IEqualityComparer<TKey>? equalityComparer = null)
            where TKey : notnull => configuration.Distinct(getKey, equalityComparer, out _);

        public static DecoratorsConfiguration<T> Distinct<T, TKey>(this DecoratorsConfiguration<T> configuration, Func<T, Optional<TKey>> getKey,
            IEqualityComparer<TKey>? equalityComparer, out DistinctCollectionDecorator<T, TKey> decorator)
            where TKey : notnull
        {
            decorator = new DistinctCollectionDecorator<T, TKey>(configuration.Priority, configuration.AllowNull, getKey, equalityComparer);
            return configuration.Add(decorator).UpdatePriority();
        }

        public static DecoratorsConfiguration<T> GroupBy<T, TKey, TGroup>(this DecoratorsConfiguration<T> configuration, Func<T, Optional<TKey>> getKey,
            Func<TKey, TGroup> getGroup, IComparer<TGroup>? sortingComparer = null, IEqualityComparer<TKey>? comparer = null, IEqualityComparer<T>? comparerValue = null,
            bool flatten = true, bool flattenDecoratedItems = true)
            where TKey : notnull
            where TGroup : class
        {
            configuration = configuration.GroupBy(getKey, getGroup, comparer, comparerValue);
            if (sortingComparer != null)
                configuration = configuration.For<TGroup>().OrderBy(sortingComparer).For<T>();
            return configuration.FlattenGroup<T, TGroup>(flatten, flattenDecoratedItems);
        }

        public static DecoratorsConfiguration<T> GroupBy<T, TKey, TGroup>(this DecoratorsConfiguration<T> configuration, Func<T, Optional<TKey>> getKey,
            Func<TKey, TGroup> getGroup, SortingComparerBuilder.BuilderDelegate<TGroup>? getSortingComparer, IEqualityComparer<TKey>? comparer = null,
            IEqualityComparer<T>? comparerValue = null, bool flatten = true, bool flattenDecoratedItems = true)
            where TKey : notnull
            where TGroup : class
        {
            configuration = configuration.GroupBy(getKey, getGroup, comparer, comparerValue);
            if (getSortingComparer != null)
                configuration = configuration.For<TGroup>().OrderBy(getSortingComparer).For<T>();
            return configuration.FlattenGroup<T, TGroup>(flatten, flattenDecoratedItems);
        }

        public static DecoratorsConfiguration<T> GroupBy<T, TKey, TGroup, TSortState>(this DecoratorsConfiguration<T> configuration, Func<T, Optional<TKey>> getKey,
            Func<TKey, TGroup> getGroup, Func<TGroup, TSortState> getSortState, SortingComparerBuilder.BuilderDelegate<TSortState?> getSortingComparer,
            IEqualityComparer<TKey>? comparer = null, IEqualityComparer<T>? comparerValue = null, bool flatten = true, bool flattenDecoratedItems = true)
            where TKey : notnull
            where TGroup : class
        {
            return configuration.GroupBy(getKey, getGroup, comparer, comparerValue)
                                .For<TGroup>()
                                .OrderBy(getSortState, getSortingComparer)
                                .For<T>()
                                .FlattenGroup<T, TGroup>(flatten, flattenDecoratedItems);
        }

        public static DecoratorsConfiguration<T> GroupBy<T, TKey, TGroup, TSortState>(this DecoratorsConfiguration<T> configuration, Func<T, Optional<TKey>> getKey,
            Func<TKey, TGroup> getGroup, Func<TGroup, TSortState> getSortState, IComparer<TSortState?>? sortingComparer = null, IEqualityComparer<TKey>? comparer = null,
            IEqualityComparer<T>? comparerValue = null, bool flatten = true, bool flattenDecoratedItems = true)
            where TKey : notnull
            where TGroup : class
        {
            return configuration.GroupBy(getKey, getGroup, comparer, comparerValue)
                                .For<TGroup>()
                                .OrderBy(getSortState, sortingComparer)
                                .For<T>()
                                .FlattenGroup<T, TGroup>(flatten, flattenDecoratedItems);
        }

        public static DecoratorsConfiguration<T> GroupBy<T, TKey, TGroup>(this DecoratorsConfiguration<T> configuration, Func<T, TKey?> getKey,
            Func<TKey, TGroup> getGroup, UpdateGroupDelegate<T, TKey, TGroup>? updateGroup, IEqualityComparer<TKey>? comparer = null, IEqualityComparer<T>? comparerValue = null)
            where TKey : notnull
            where TGroup : class => configuration.GroupBy(getKey.OptionalClosure, getGroup, updateGroup, comparer, comparerValue, out _);

        public static DecoratorsConfiguration<T> GroupBy<T, TKey, TGroup>(this DecoratorsConfiguration<T> configuration, Func<T, TKey?> getKey,
            Func<TKey, TGroup> getGroup, UpdateGroupDelegate<T, TKey, TGroup>? updateGroup, IEqualityComparer<TKey>? comparer, IEqualityComparer<T>? comparerValue,
            out GroupCollectionDecorator<T, TKey, TGroup> decorator)
            where TKey : notnull
            where TGroup : class =>
            configuration.GroupBy(getKey.OptionalClosure, getGroup, updateGroup, comparer, comparerValue, out decorator);

        public static DecoratorsConfiguration<T> GroupBy<T, TKey, TGroup>(this DecoratorsConfiguration<T> configuration, Func<T, Optional<TKey>> getKey,
            Func<TKey, TGroup> getGroup, UpdateGroupDelegate<T, TKey, TGroup>? updateGroup, IEqualityComparer<TKey>? comparer = null, IEqualityComparer<T>? comparerValue = null)
            where TKey : notnull
            where TGroup : class => configuration.GroupBy(getKey, getGroup, updateGroup, comparer, comparerValue, out _);

        public static DecoratorsConfiguration<T> GroupBy<T, TKey, TGroup>(this DecoratorsConfiguration<T> configuration, Func<T, Optional<TKey>> getKey,
            Func<TKey, TGroup> getGroup, UpdateGroupDelegate<T, TKey, TGroup>? updateGroup, IEqualityComparer<TKey>? comparer, IEqualityComparer<T>? comparerValue,
            out GroupCollectionDecorator<T, TKey, TGroup> decorator)
            where TKey : notnull
            where TGroup : class
        {
            decorator = new GroupCollectionDecorator<T, TKey, TGroup>(configuration.Priority, configuration.AllowNull, getKey, getGroup, updateGroup, comparer, comparerValue);
            return configuration.Add(decorator);
        }

        public static DecoratorsConfiguration<T> WithDynamicHeader<T, TKey, TGroup>(this DecoratorsConfiguration<T> configuration, Func<T, TKey?> getKey,
            Func<TKey, TGroup> getHeader, IEqualityComparer<TKey>? comparer = null)
            where TKey : notnull
            where TGroup : class =>
            configuration.WithDynamicHeader(getKey.OptionalClosure, getHeader, comparer, out _);

        public static DecoratorsConfiguration<T> WithDynamicHeader<T, TKey, TGroup>(this DecoratorsConfiguration<T> configuration, Func<T, Optional<TKey>> getKey,
            Func<TKey, TGroup> getHeader, IEqualityComparer<TKey>? comparer = null)
            where TKey : notnull
            where TGroup : class =>
            configuration.WithDynamicHeader(getKey, getHeader, comparer, out _);

        public static DecoratorsConfiguration<T> WithDynamicHeader<T, TKey, TGroup>(this DecoratorsConfiguration<T> configuration, Func<T, Optional<TKey>> getKey,
            Func<TKey, TGroup> getHeader, IEqualityComparer<TKey>? comparer,
            out ActionToken removeToken)
            where TKey : notnull
            where TGroup : class =>
            configuration.Add(new GroupCollectionDecorator<T, TKey, TGroup>(configuration.Priority, configuration.AllowNull, getKey, getHeader, null, comparer, null), null,
                out removeToken);

        public static DecoratorsConfiguration<T> WithHeaderFooter<T>(this DecoratorsConfiguration<T> configuration, ItemOrIReadOnlyList<object> header,
            ItemOrIReadOnlyList<object> footer = default) => configuration.WithHeaderFooter(header, footer, out _);

        public static DecoratorsConfiguration<T> WithHeaderFooter<T>(this DecoratorsConfiguration<T> configuration, ItemOrIReadOnlyList<object> header,
            ItemOrIReadOnlyList<object> footer, out HeaderFooterCollectionDecorator decorator)
        {
            decorator = new HeaderFooterCollectionDecorator(configuration.Priority).SetHeaderFooter(header, footer);
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

        public static DecoratorsConfiguration<T> Subscribe<T>(this DecoratorsConfiguration<T> configuration,
            Action<TrackerCollectionDecorator<T, object?>, T>? onAdded, Action<TrackerCollectionDecorator<T, object?>, T>? onRemoved = null,
            Action<TrackerCollectionDecorator<T, object?>, T, object?>? onChanged = null, Action<TrackerCollectionDecorator<T, object?>>? onReset = null,
            Func<T, bool>? immutableCondition = null, IEqualityComparer<T>? comparer = null) =>
            configuration.Subscribe(onAdded, onRemoved, onChanged, onReset, immutableCondition, comparer, out _);

        public static DecoratorsConfiguration<T> Subscribe<T>(this DecoratorsConfiguration<T> configuration,
            Action<TrackerCollectionDecorator<T, object?>, T>? onAdded, Action<TrackerCollectionDecorator<T, object?>, T>? onRemoved,
            Action<TrackerCollectionDecorator<T, object?>, T, object?>? onChanged, Action<TrackerCollectionDecorator<T, object?>>? onReset,
            Func<T, bool>? immutableCondition, IEqualityComparer<T>? comparer, out TrackerCollectionDecorator<T, object?> decorator)
        {
            decorator = new TrackerCollectionDecorator<T, object?>(configuration.Priority, configuration.AllowNull,
                onAdded == null ? NoDoSubscribeDelegate<T>() : onAdded.OnAddedClosure,
                onRemoved == null ? NoDoSubscribeDelegate<T>() : onRemoved.OnRemovedClosure, onChanged == null ? null : onChanged.OnChangedClosure, onReset, immutableCondition,
                comparer);
            return configuration.Add(decorator);
        }

        public static DecoratorsConfiguration<T> Materialize<T>(this DecoratorsConfiguration<T> configuration) =>
            configuration.Add(new MaterializeCollectionDecorator(configuration.Priority));

        public static DecoratorsConfiguration<T> Materialize<T>(this DecoratorsConfiguration<T> configuration, out ActionToken removeToken) =>
            configuration.Add(new MaterializeCollectionDecorator(configuration.Priority), null, out removeToken);

        public static DecoratorsConfiguration<T> Bind<T>(this DecoratorsConfiguration<T> configuration, out IReadOnlyObservableCollection<T> collection,
            bool disposeSourceOnDispose = false, bool isWeak = true, bool materialize = false)
        {
            collection = new DecoratedReadOnlyObservableCollection<T>(configuration.Collection, configuration.Priority, disposeSourceOnDispose, isWeak, materialize);
            return configuration.UpdatePriority();
        }

        public static IReadOnlyObservableCollection<T> Bind<T>(this DecoratorsConfiguration<T> configuration, bool disposeSourceOnDispose = false, bool isWeak = true,
            bool materialize = false)
        {
            configuration.Bind(out var collection, disposeSourceOnDispose, isWeak, materialize);
            return collection;
        }

        public static DecoratorsConfiguration<T> TrackSelectedItem<T>(this DecoratorsConfiguration<T> configuration, out ISelectedItemTracker<T> tracker,
            NotifyPropertyChangedBase source, string propertyName = nameof(ISelectedItemTracker<object>.SelectedItem), Func<IReadOnlyCollection<T>, T?, T?>? getDefault = null,
            Func<T, bool>? immutableCondition = null, IEqualityComparer<T>? comparer = null)
            where T : class
        {
            Should.NotBeNull(source, nameof(source));
            var args = propertyName == nameof(ISelectedItemTracker<object>.SelectedItem) ? Default.SelectedItemChangedArgs : new PropertyChangedEventArgs(propertyName);
            return configuration.TrackSelectedItem(out tracker, (args, source), (_, s, _) => s.source.OnPropertyChangedInternal(s.args), getDefault, immutableCondition, comparer);
        }

        public static DecoratorsConfiguration<T> TrackSelectedItem<T>(this DecoratorsConfiguration<T> configuration, out ISelectedItemTracker<T> tracker,
            Func<IReadOnlyCollection<T>, T?, T?>? getDefault = null, Func<T, bool>? immutableCondition = null, IEqualityComparer<T>? comparer = null)
            where T : class =>
            configuration.TrackSelectedItem<T, object?>(out tracker, null, null, getDefault, immutableCondition, comparer);

        public static DecoratorsConfiguration<T> TrackSelectedItem<T, TState>(this DecoratorsConfiguration<T> configuration, out ISelectedItemTracker<T> tracker,
            TState state = default!, Action<T?, TState, IReadOnlyMetadataContext?>? onChanged = null, Func<IReadOnlyCollection<T>, T?, T?>? getDefault = null,
            Func<T, bool>? immutableCondition = null,
            IEqualityComparer<T>? comparer = null)
            where T : class
        {
            var closure = new SelectedItemTracker<T, TState>(configuration.Collection, getDefault, onChanged, state);
            var decorator = new TrackerCollectionDecorator<T, object?>(configuration.Priority, configuration.AllowNull, closure.OnAdded, closure.OnRemoved, null,
                closure.OnEndBatchUpdate, immutableCondition, comparer);
            closure.Tracker = decorator;
            tracker = closure;
            return configuration.Add(decorator);
        }

        public static DecoratorsConfiguration<T> AutoRefreshOnPropertyChanged<T>(this DecoratorsConfiguration<T> configuration, ItemOrArray<string> members, object? args = null,
            Func<T, bool>? immutableCondition = null)
            where T : class => configuration.AutoRefreshOnPropertyChanged(members, args, immutableCondition, out _);

        public static DecoratorsConfiguration<T> AutoRefreshOnPropertyChanged<T>(this DecoratorsConfiguration<T> configuration, ItemOrArray<string> members,
            object? args, Func<T, bool>? immutableCondition, out ActionToken removeToken) where T : class
        {
            var collection = configuration.Collection;
            PropertyChangedEventHandler handler = (sender, e) =>
            {
                if (sender != null && string.IsNullOrEmpty(e.PropertyName) || members.Contains(e.PropertyName!))
                    collection.RaiseItemChanged(sender, args ?? e);
            };
            return configuration.Subscribe<PropertyChangedEventHandler?>((_, item, _, count) =>
            {
                if (count == 1 && item is INotifyPropertyChanged notifyPropertyChanged)
                {
                    notifyPropertyChanged.PropertyChanged += handler;
                    return handler;
                }

                return null;
            }, (_, item, state, count) =>
            {
                if (count == 0 && state != null)
                    ((INotifyPropertyChanged) item).PropertyChanged -= state;
                return state;
            }, null, null, immutableCondition, InternalEqualityComparer.Reference, out var decorator).WithRemoveToken(decorator, out removeToken);
        }

        public static DecoratorsConfiguration<T> AutoRefreshOnObservable<T, TSignal>(this DecoratorsConfiguration<T> configuration, Func<T, IObservable<TSignal>> getObservable,
            object? args = null, Func<T, bool>? immutableCondition = null) where T : class => configuration.AutoRefreshOnObservable(getObservable, args, immutableCondition, out _);

        public static DecoratorsConfiguration<T> AutoRefreshOnObservable<T, TSignal>(this DecoratorsConfiguration<T> configuration, Func<T, IObservable<TSignal>> getObservable,
            object? args, Func<T, bool>? immutableCondition, out ActionToken removeToken) where T : class
        {
            Should.NotBeNull(getObservable, nameof(getObservable));
            var collection = configuration.Collection;
            return configuration.Subscribe<IDisposable>((_, item, state, _) =>
            {
                if (state == null)
                    return getObservable(item).Subscribe(new ItemChangedObserver<TSignal>(collection, item, args));
                return state;
            }, (_, _, state, count) =>
            {
                if (count == 0)
                    state.Dispose();
                return state;
            }, null, null, immutableCondition, InternalEqualityComparer.Reference, out var decorator).WithRemoveToken(decorator, out removeToken);
        }

        public static DecoratorsConfiguration<T> Min<T, TResult>(this DecoratorsConfiguration<T> configuration, Func<T, TResult> selector, Action<T?, TResult?> onChanged,
            TResult? defaultValue = default, Func<T, bool>? predicate = null, IComparer<TResult?>? comparer = null, bool isImmutable = false) =>
            MaxMin(configuration, selector, onChanged, defaultValue, predicate, comparer, false, isImmutable, out _);

        public static DecoratorsConfiguration<T> Min<T, TResult>(this DecoratorsConfiguration<T> configuration, Func<T, TResult> selector, Action<T?, TResult?> onChanged,
            TResult? defaultValue, Func<T, bool>? predicate, IComparer<TResult?>? comparer, bool isImmutable, out ActionToken removeToken) =>
            MaxMin(configuration, selector, onChanged, defaultValue, predicate, comparer, false, isImmutable, out removeToken);

        public static DecoratorsConfiguration<T> Max<T, TResult>(this DecoratorsConfiguration<T> configuration, Func<T, TResult> selector, Action<T?, TResult?> onChanged,
            TResult? defaultValue = default, Func<T, bool>? predicate = null, IComparer<TResult?>? comparer = null, bool isImmutable = false) =>
            MaxMin(configuration, selector, onChanged, defaultValue, predicate, comparer, true, isImmutable, out _);

        public static DecoratorsConfiguration<T> Max<T, TResult>(this DecoratorsConfiguration<T> configuration, Func<T, TResult> selector, Action<T?, TResult?> onChanged,
            TResult? defaultValue, Func<T, bool>? predicate, IComparer<TResult?>? comparer, bool isImmutable, out ActionToken removeToken) =>
            MaxMin(configuration, selector, onChanged, defaultValue, predicate, comparer, true, isImmutable, out removeToken);

        public static DecoratorsConfiguration<T> MaxMin<T, TResult>(DecoratorsConfiguration<T> configuration, Func<T, TResult> selector, Action<T?, TResult?> onChanged,
            TResult? defaultValue, Func<T, bool>? predicate, IComparer<TResult?>? comparer, bool isMax, bool isImmutable, out ActionToken removeToken)
        {
            Should.NotBeNull(selector, nameof(selector));
            Should.NotBeNull(onChanged, nameof(onChanged));
            if (isImmutable)
            {
                var d = new MaxMinImmutableCollectionDecorator<T, TResult>(configuration.Priority, configuration.AllowNull, defaultValue, selector, onChanged, comparer, isMax,
                    predicate);
                return configuration.Add(d, null, out removeToken);
            }

            var closure = new MaxMinClosure<T, TResult>(defaultValue, selector, onChanged, comparer, isMax, predicate);
            return configuration.Subscribe<(TResult?, bool)>(closure.OnAdded, closure.OnRemoved, closure.OnChanged, closure.OnEndBatchUpdate, null, null, out var decorator)
                                .WithRemoveToken(decorator, out removeToken);
        }

        public static DecoratorsConfiguration<T> Count<T>(this DecoratorsConfiguration<T> configuration, Action<int> onChanged, Func<T, bool>? predicate = null,
            bool isImmutable = false) => configuration.Count(onChanged, predicate, isImmutable, out _);

        public static DecoratorsConfiguration<T> Count<T>(this DecoratorsConfiguration<T> configuration, Action<int> onChanged, Func<T, bool>? predicate,
            bool isImmutable, out ActionToken removeToken) =>
            configuration.Accumulate(0, _ => 1, (x1, x2) => x1 + x2, (x1, x2) => x1 - x2, onChanged, predicate, isImmutable || predicate == null, out removeToken);

        public static DecoratorsConfiguration<T> Sum<T>(this DecoratorsConfiguration<T> configuration, Func<T, int> selector, Action<int> onChanged,
            Func<T, bool>? predicate = null, bool isImmutable = false) => configuration.Sum(selector, onChanged, predicate, isImmutable, out _);

        public static DecoratorsConfiguration<T> Sum<T>(this DecoratorsConfiguration<T> configuration, Func<T, int?> selector, Action<int?> onChanged,
            Func<T, bool>? predicate = null, bool isImmutable = false) => configuration.Sum(selector, onChanged, predicate, isImmutable, out _);

        public static DecoratorsConfiguration<T> Sum<T>(this DecoratorsConfiguration<T> configuration, Func<T, long> selector, Action<long> onChanged,
            Func<T, bool>? predicate = null, bool isImmutable = false) => configuration.Sum(selector, onChanged, predicate, isImmutable, out _);

        public static DecoratorsConfiguration<T> Sum<T>(this DecoratorsConfiguration<T> configuration, Func<T, long?> selector, Action<long?> onChanged,
            Func<T, bool>? predicate = null, bool isImmutable = false) => configuration.Sum(selector, onChanged, predicate, isImmutable, out _);

        public static DecoratorsConfiguration<T> Sum<T>(this DecoratorsConfiguration<T> configuration, Func<T, float> selector, Action<float> onChanged,
            Func<T, bool>? predicate = null, bool isImmutable = false) => configuration.Sum(selector, onChanged, predicate, isImmutable, out _);

        public static DecoratorsConfiguration<T> Sum<T>(this DecoratorsConfiguration<T> configuration, Func<T, float?> selector, Action<float?> onChanged,
            Func<T, bool>? predicate = null, bool isImmutable = false) => configuration.Sum(selector, onChanged, predicate, isImmutable, out _);

        public static DecoratorsConfiguration<T> Sum<T>(this DecoratorsConfiguration<T> configuration, Func<T, double> selector, Action<double> onChanged,
            Func<T, bool>? predicate = null, bool isImmutable = false) => configuration.Sum(selector, onChanged, predicate, isImmutable, out _);

        public static DecoratorsConfiguration<T> Sum<T>(this DecoratorsConfiguration<T> configuration, Func<T, double?> selector, Action<double?> onChanged,
            Func<T, bool>? predicate = null, bool isImmutable = false) => configuration.Sum(selector, onChanged, predicate, isImmutable, out _);

        public static DecoratorsConfiguration<T> Sum<T>(this DecoratorsConfiguration<T> configuration, Func<T, decimal> selector, Action<decimal> onChanged,
            Func<T, bool>? predicate = null, bool isImmutable = false) => configuration.Sum(selector, onChanged, predicate, isImmutable, out _);

        public static DecoratorsConfiguration<T> Sum<T>(this DecoratorsConfiguration<T> configuration, Func<T, decimal?> selector, Action<decimal?> onChanged,
            Func<T, bool>? predicate = null, bool isImmutable = false) => configuration.Sum(selector, onChanged, predicate, isImmutable, out _);

        public static DecoratorsConfiguration<T> Sum<T>(this DecoratorsConfiguration<T> configuration, Func<T, int> selector, Action<int> onChanged, Func<T, bool>? predicate,
            bool isImmutable, out ActionToken removeToken) =>
            configuration.Accumulate(0, selector, (x1, x2) => x1 + x2, (x1, x2) => x1 - x2, onChanged, predicate, isImmutable, out removeToken);

        public static DecoratorsConfiguration<T> Sum<T>(this DecoratorsConfiguration<T> configuration, Func<T, int?> selector, Action<int?> onChanged, Func<T, bool>? predicate,
            bool isImmutable, out ActionToken removeToken) =>
            configuration.Accumulate(0, selector, (x1, x2) => x1 + x2, (x1, x2) => x1 - x2, onChanged, predicate, isImmutable, out removeToken);

        public static DecoratorsConfiguration<T> Sum<T>(this DecoratorsConfiguration<T> configuration, Func<T, long> selector, Action<long> onChanged, Func<T, bool>? predicate,
            bool isImmutable, out ActionToken removeToken) =>
            configuration.Accumulate(0, selector, (x1, x2) => x1 + x2, (x1, x2) => x1 - x2, onChanged, predicate, isImmutable, out removeToken);

        public static DecoratorsConfiguration<T> Sum<T>(this DecoratorsConfiguration<T> configuration, Func<T, long?> selector, Action<long?> onChanged, Func<T, bool>? predicate,
            bool isImmutable, out ActionToken removeToken) =>
            configuration.Accumulate(0, selector, (x1, x2) => x1 + x2, (x1, x2) => x1 - x2, onChanged, predicate, isImmutable, out removeToken);

        public static DecoratorsConfiguration<T> Sum<T>(this DecoratorsConfiguration<T> configuration, Func<T, float> selector, Action<float> onChanged, Func<T, bool>? predicate,
            bool isImmutable, out ActionToken removeToken) =>
            configuration.Accumulate(0, selector, (x1, x2) => x1 + x2, (x1, x2) => x1 - x2, onChanged, predicate, isImmutable, out removeToken);

        public static DecoratorsConfiguration<T> Sum<T>(this DecoratorsConfiguration<T> configuration, Func<T, float?> selector, Action<float?> onChanged, Func<T, bool>? predicate,
            bool isImmutable, out ActionToken removeToken) =>
            configuration.Accumulate(0, selector, (x1, x2) => x1 + x2, (x1, x2) => x1 - x2, onChanged, predicate, isImmutable, out removeToken);

        public static DecoratorsConfiguration<T> Sum<T>(this DecoratorsConfiguration<T> configuration, Func<T, double> selector, Action<double> onChanged, Func<T, bool>? predicate,
            bool isImmutable, out ActionToken removeToken) =>
            configuration.Accumulate(0, selector, (x1, x2) => x1 + x2, (x1, x2) => x1 - x2, onChanged, predicate, isImmutable, out removeToken);

        public static DecoratorsConfiguration<T> Sum<T>(this DecoratorsConfiguration<T> configuration, Func<T, double?> selector, Action<double?> onChanged,
            Func<T, bool>? predicate, bool isImmutable, out ActionToken removeToken) =>
            configuration.Accumulate(0, selector, (x1, x2) => x1 + x2, (x1, x2) => x1 - x2, onChanged, predicate, isImmutable, out removeToken);

        public static DecoratorsConfiguration<T> Sum<T>(this DecoratorsConfiguration<T> configuration, Func<T, decimal> selector, Action<decimal> onChanged,
            Func<T, bool>? predicate, bool isImmutable, out ActionToken removeToken) =>
            configuration.Accumulate(0, selector, (x1, x2) => x1 + x2, (x1, x2) => x1 - x2, onChanged, predicate, isImmutable, out removeToken);

        public static DecoratorsConfiguration<T> Sum<T>(this DecoratorsConfiguration<T> configuration, Func<T, decimal?> selector, Action<decimal?> onChanged,
            Func<T, bool>? predicate, bool isImmutable, out ActionToken removeToken) =>
            configuration.Accumulate(0, selector, (x1, x2) => x1 + x2, (x1, x2) => x1 - x2, onChanged, predicate, isImmutable, out removeToken);

        public static DecoratorsConfiguration<T> All<T>(this DecoratorsConfiguration<T> configuration, Func<T, bool> selector, Action<bool> onChanged,
            Func<T, bool>? predicate = null, bool isImmutable = false) => configuration.All(selector, onChanged, predicate, isImmutable, out _);

        public static DecoratorsConfiguration<T> All<T>(this DecoratorsConfiguration<T> configuration, Func<T, bool> selector, Action<bool> onChanged, Func<T, bool>? predicate,
            bool isImmutable, out ActionToken removeToken)
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
                }, predicate, isImmutable, out removeToken);
        }

        public static DecoratorsConfiguration<T> Any<T>(this DecoratorsConfiguration<T> configuration, Action<bool> onChanged, Func<T, bool>? selector = null) =>
            configuration.Any(onChanged, selector, out _);

        public static DecoratorsConfiguration<T> Any<T>(this DecoratorsConfiguration<T> configuration, Action<bool> onChanged, Func<T, bool>? selector,
            out ActionToken removeToken)
        {
            Should.NotBeNull(onChanged, nameof(onChanged));
            return configuration.FirstOrDefault(onChanged.AnyCallback, selector, out removeToken);
        }

        public static DecoratorsConfiguration<T> Accumulate<T, TResult>(this DecoratorsConfiguration<T> configuration, TResult seed,
            Func<T, TResult> selector, Func<TResult, TResult, TResult> add, Func<TResult, TResult, TResult> remove, Action<TResult> onChanged, Func<T, bool>? predicate = null,
            bool isImmutable = false) => configuration.Accumulate(seed, selector, add, remove, onChanged, predicate, isImmutable, out _);

        public static DecoratorsConfiguration<T> Accumulate<T, TResult>(this DecoratorsConfiguration<T> configuration, TResult seed,
            Func<T, TResult> selector, Func<TResult, TResult, TResult> add, Func<TResult, TResult, TResult> remove, Action<TResult> onChanged, Func<T, bool>? predicate,
            bool isImmutable,
            out ActionToken removeToken)
        {
            Should.NotBeNull(selector, nameof(selector));
            Should.NotBeNull(add, nameof(add));
            Should.NotBeNull(remove, nameof(remove));
            Should.NotBeNull(onChanged, nameof(onChanged));
            if (isImmutable)
            {
                var d = new AccumulateImmutableCollectionDecorator<T, TResult>(configuration.Priority, configuration.AllowNull, seed, selector, add, remove, onChanged, predicate);
                return configuration.Add(d, null, out removeToken);
            }

            var closure = new AccumulateClosure<T, TResult>(seed, selector, add, remove, onChanged, predicate);
            return configuration.Subscribe<(TResult, bool)>(closure.OnAdded, closure.OnRemoved, closure.OnChanged, closure.OnEndBatchUpdate, null, null, out var decorator)
                                .WithRemoveToken(decorator, out removeToken);
        }

        public static DecoratorsConfiguration<T> FirstOrDefault<T>(this DecoratorsConfiguration<T> configuration, Action<Optional<T?>> setter, Func<T, bool>? predicate = null) =>
            configuration.FirstOrDefault(setter, predicate, out _);

        public static DecoratorsConfiguration<T> FirstOrDefault<T>(this DecoratorsConfiguration<T> configuration, Action<Optional<T?>> setter, Func<T, bool>? predicate,
            out ActionToken removeToken) => configuration.Add(new FirstLastTrackerCollectionDecorator<T>(configuration.Priority, configuration.AllowNull, true, setter, predicate),
            null, out removeToken);

        public static DecoratorsConfiguration<T> LastOrDefault<T>(this DecoratorsConfiguration<T> configuration, Action<Optional<T?>> setter, Func<T, bool>? predicate = null) =>
            configuration.LastOrDefault(setter, predicate, out _);

        public static DecoratorsConfiguration<T> LastOrDefault<T>(this DecoratorsConfiguration<T> configuration, Action<Optional<T?>> setter, Func<T, bool>? predicate,
            out ActionToken removeToken) => configuration.Add(new FirstLastTrackerCollectionDecorator<T>(configuration.Priority, configuration.AllowNull, false, setter, predicate),
            null, out removeToken);

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
            var result = new Dictionary<TKey, TValue>(dictionary.Count, comparer);
            foreach (var value in dictionary)
                result[value.Key] = value.Value;
#else
            var result = new Dictionary<TKey, TValue>(dictionary, comparer);
#endif
            foreach (var value in values)
                result[value.Key] = value.Value;
            return result;
        }

#if NET461
        internal static bool Remove<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, [MaybeNullWhen(false)] out TValue value) where TKey : notnull
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

        internal static void FindAllIndexOf(this IEnumerable items, object? item, bool ignoreDuplicates, ref ItemOrListEditor<int> indexes)
        {
            if (items is IHasFindAllIndexOfSupport hasIndexOfSupport)
            {
                hasIndexOfSupport.FindAllIndexOf(item, ignoreDuplicates, ref indexes);
                return;
            }

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
            foreach (var value in items.AsEnumerable())
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
            return enumerable as IReadOnlyList<object> ?? enumerable.AsEnumerable().ToList()!;
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

        internal static bool Add<T>(ref ImmutableHashSet<T> hashSet, T item) => ImmutableInterlocked.Update(ref hashSet, (set, i) => set.Add(i), item);

        internal static bool Remove<T>(ref ImmutableHashSet<T> hashSet, T item) => ImmutableInterlocked.Update(ref hashSet, (set, i) => set.Remove(i), item);

        private static DecoratorsConfiguration<T> GroupBy<T, TKey, TGroup>(this DecoratorsConfiguration<T> configuration, Func<T, Optional<TKey>> getKey,
            Func<TKey, TGroup> getGroup, IEqualityComparer<TKey>? comparer, IEqualityComparer<T>? comparerValue)
            where TKey : notnull
            where TGroup : class
        {
            return configuration.GroupBy(getKey, getGroup, (_, group, items, action, item, args) =>
                                {
                                    if (group is ICollectionGroup<T> g && (action != CollectionGroupChangedAction.GroupRemoved || !g.TryCleanup()))
                                        action.ApplyChangesTo(g.Items, items, item, args);
                                }, comparer, comparerValue)
                                .Where(_ => false);
        }

        private static DecoratorsConfiguration<T> FlattenGroup<T, TGroup>(this DecoratorsConfiguration<T> configuration, bool flatten, bool flattenDecoratedItems)
            where TGroup : class
        {
            if (!flatten)
                return configuration;

            return configuration.For<TGroup>()
                                .SelectMany(flattenDecoratedItems
                                    ? (group, _) => new FlattenItemInfo((group as ICollectionGroup<T>)?.Items, true)
                                    : (group, _) => new FlattenItemInfo((group as ICollectionGroup<T>)?.Items, false))
                                .For<T>();
        }

        private static FlattenItemInfo FlattenDecorated(this IEnumerable enumerable, IEnumerable value, FlattenItemInfo itemInfo)
        {
            if (ReferenceEquals(enumerable, value))
                return new FlattenItemInfo(enumerable, true);
            return default;
        }

        private static FlattenItemInfo Flatten(this IEnumerable enumerable, IEnumerable value, FlattenItemInfo itemInfo)
        {
            if (ReferenceEquals(enumerable, value))
                return new FlattenItemInfo(enumerable, false);
            return default;
        }

        private static FlattenItemInfo SelectManyDecorated<T>(this Func<T, IEnumerable?> selector, T item, FlattenItemInfo currentItem) => new(selector(item), true);

        private static FlattenItemInfo SelectMany<T>(this Func<T, IEnumerable?> selector, T item, FlattenItemInfo currentItem) => new(selector(item), false);

        private static FlattenItemInfo SelectManyDecoratedRecycle<T>(this Func<T, IEnumerable?> selector, T item, FlattenItemInfo currentItem) =>
            currentItem.IsEmpty(null) ? new FlattenItemInfo(selector(item), true) : currentItem;

        private static FlattenItemInfo SelectManyRecycle<T>(this Func<T, IEnumerable?> selector, T item, FlattenItemInfo currentItem) =>
            currentItem.IsEmpty(null) ? new FlattenItemInfo(selector(item), false) : currentItem;

        private static (int total, int count) AllSelector<T>(this Func<T, bool> selector, T item) => (1, selector(item) ? 1 : 0);

        private static void AnyCallback<T>(this Action<bool> onChanged, Optional<T> value) => onChanged(value.HasValue);

        private static Optional<TKey> OptionalClosure<T, TKey>(this Func<T, TKey?> getKey, T value) where TKey : notnull => Optional.Get(getKey(value));

        private static Optional<TResult> OptionalClosure<T, TResult>(this Func<T, TResult?, TResult> select, T value, TResult? convertValue)
            where TResult : class? => select(value, convertValue);

        private static TState? GetStateOrderBy<T, TState>(this Func<T, TState?> getState, object? item)
        {
            if (item is T itemT)
                return getState(itemT);
            return default;
        }

        private static bool FilterWhere<T>(this Func<T, bool> filter, T item, int _) => filter(item);

        private static object? OnAddedClosure<T>(this Action<TrackerCollectionDecorator<T, object?>, T> onAdded, TrackerCollectionDecorator<T, object?> tracker, T item,
            object? state, int count)
        {
            if (count == 1)
                onAdded(tracker, item);
            return null;
        }

        private static object? OnRemovedClosure<T>(this Action<TrackerCollectionDecorator<T, object?>, T> onRemoved, TrackerCollectionDecorator<T, object?> tracker, T item,
            object? state, int count)
        {
            if (count == 0)
                onRemoved(tracker, item);
            return null;
        }

        private static object? OnChangedClosure<T>(this Action<TrackerCollectionDecorator<T, object?>, T, object?> onChanged, TrackerCollectionDecorator<T, object?> tracker,
            T item, object? state, int count, object? args)
        {
            onChanged(tracker, item, args);
            return null;
        }

        private static Func<TrackerCollectionDecorator<T, object?>, T, object?, int, object?> NoDoSubscribeDelegate<T>() => (_, _, _, _) => null;

#if NET5_0
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