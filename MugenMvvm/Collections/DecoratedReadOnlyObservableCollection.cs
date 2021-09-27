using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MugenMvvm.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Models.Components;
using MugenMvvm.Internal;

// ReSharper disable PossibleMultipleEnumeration

namespace MugenMvvm.Collections
{
    [DebuggerDisplay("Count={" + nameof(Count) + "}")]
    [DebuggerTypeProxy(typeof(ReadOnlyObservableCollectionDebuggerProxy<>))]
    internal sealed class DecoratedReadOnlyObservableCollection<T> : ComponentOwnerBase<IReadOnlyObservableCollection>, IReadOnlyObservableCollection<T>
    {
        private readonly IReadOnlyObservableCollection _source;
        private readonly bool _disposeSource;
        private DecoratorListener? _decorator;
        private bool _hasCount;
        private int _count;

        public DecoratedReadOnlyObservableCollection(IReadOnlyObservableCollection source, int priority, bool disposeSource, bool isWeak, bool materialize,
            IComponentCollectionManager? componentCollectionManager = null) : base(componentCollectionManager, source)
        {
            Should.NotBeNull(source, nameof(source));
            _source = source;
            _disposeSource = disposeSource;
            _decorator = new DecoratorListener(this, isWeak, materialize, priority);
            using (source.Lock())
            {
                source.AddComponent(_decorator);
                if (!_hasCount)
                {
                    var decoratorManager = source.GetComponentOptional<ICollectionDecoratorManagerComponent>();
                    if (decoratorManager != null)
                        Count = decoratorManager.Decorate(_source, _decorator).CountEx();
                }
            }
        }

        public bool IsDisposed => _decorator == null;

        public int Count
        {
            get => _count;
            private set
            {
                _count = value;
                _hasCount = true;
            }
        }

        public ILocker Locker => _source.Locker;

        Type IReadOnlyObservableCollection.ItemType => typeof(T);

        public void Dispose()
        {
            var decorator = _decorator;
            if (decorator == null)
                return;

            using var _ = Lock();
            if (_decorator == null)
                return;

            var components = GetComponents<IDisposableComponent<IReadOnlyObservableCollection>>();
            components.OnDisposing(this, null);
            _decorator = null;
            _source.RemoveComponent(decorator);
            components.OnDisposed(this, null);
            this.ClearComponents();
            Count = 0;
            if (_disposeSource)
                _source.Dispose();
        }

        public IEnumerator<T> GetEnumerator()
        {
            var decorator = _decorator;
            if (decorator == null)
                return Default.EmptyEnumerator<T>();
            return GetEnumerable(decorator).GetEnumerator();
        }

        public void UpdateLocker(ILocker locker) => _source.UpdateLocker(locker);

        public ActionToken Lock() => _source.Lock();

        public bool TryLock(int timeout, out ActionToken lockToken) => _source.TryLock(timeout, out lockToken);

        private IEnumerable<T> GetEnumerable(ICollectionDecorator decorator)
        {
            Should.NotBeNull(decorator, nameof(decorator));
            var decoratorManager = _source.GetComponentOptional<ICollectionDecoratorManagerComponent>();
            if (decoratorManager == null)
                yield break;

            using var l = Lock();
            foreach (T? o in decoratorManager.Decorate(_source, decorator))
                yield return o!;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private sealed class DecoratorListener : IListenerCollectionDecorator, IFlattenCollectionListener, ICollectionBatchUpdateListener,
            ILockerChangedListener<IReadOnlyObservableCollection>, IDisposableComponent<IReadOnlyObservableCollection>, IDetachableComponent, IHasPriority
        {
            private readonly bool _materialize;
            private readonly object _target;

            public DecoratorListener(DecoratedReadOnlyObservableCollection<T> target, bool isWeak, bool materialize, int priority)
            {
                _materialize = materialize;
                Priority = priority;
                _target = isWeak ? target.ToWeakReference() : target;
            }

            public int Priority { get; }

            public void OnBeginBatchUpdate(IReadOnlyObservableCollection collection, BatchUpdateType batchUpdateType)
            {
                if (batchUpdateType != BatchUpdateType.Decorators)
                    return;
                var target = TryGetTarget(collection);
                target?.GetBatchUpdateManager().BeginBatchUpdate(target, batchUpdateType);
            }

            public void OnEndBatchUpdate(IReadOnlyObservableCollection collection, BatchUpdateType batchUpdateType)
            {
                if (batchUpdateType != BatchUpdateType.Decorators)
                    return;
                var target = TryGetTarget(collection);
                target?.GetBatchUpdateManager().EndBatchUpdate(target, batchUpdateType);
            }

            public bool IsLazy(IReadOnlyObservableCollection collection) => false;

            public bool IsCacheRequired(IReadOnlyObservableCollection collection) => _materialize;

            public bool HasAdditionalItems(IReadOnlyObservableCollection collection) => false;

            public bool TryGetIndexes(IReadOnlyObservableCollection collection, IEnumerable<object?> items, object? item, bool ignoreDuplicates,
                ref ItemOrListEditor<int> indexes) => false;

            public IEnumerable<object?> Decorate(IReadOnlyObservableCollection collection, IEnumerable<object?> items) => items;

            public bool OnChanged(IReadOnlyObservableCollection collection, ref object? item, ref int index, ref object? args)
            {
                TryGetTarget(collection)?.RaiseItemChanged(item, args);
                return true;
            }

            public bool OnAdded(IReadOnlyObservableCollection collection, ref object? item, ref int index)
            {
                var target = TryGetTarget(collection);
                if (target != null)
                {
                    ++target.Count;
                    target.GetComponents<ICollectionChangedListener<T>>().OnAdded(target, (T) item!, index);
                }

                return true;
            }

            public bool OnReplaced(IReadOnlyObservableCollection collection, ref object? oldItem, ref object? newItem, ref int index)
            {
                var target = TryGetTarget(collection);
                target?.GetComponents<ICollectionChangedListener<T>>().OnReplaced(target, (T) oldItem!, (T) newItem!, index);
                return true;
            }

            public bool OnMoved(IReadOnlyObservableCollection collection, ref object? item, ref int oldIndex, ref int newIndex)
            {
                var target = TryGetTarget(collection);
                target?.GetComponents<ICollectionChangedListener<T>>().OnMoved(target, (T) item!, oldIndex, newIndex);
                return true;
            }

            public bool OnRemoved(IReadOnlyObservableCollection collection, ref object? item, ref int index)
            {
                var target = TryGetTarget(collection);
                if (target != null)
                {
                    --target.Count;
                    target.GetComponents<ICollectionChangedListener<T>>().OnRemoved(target, (T) item!, index);
                }

                return true;
            }

            public bool OnReset(IReadOnlyObservableCollection collection, ref IEnumerable<object?>? items)
            {
                var target = TryGetTarget(collection);
                if (target != null)
                {
                    if (items == null)
                    {
                        target.Count = 0;
                        target.GetComponents<ICollectionChangedListener<T>>().OnReset(target, null);
                    }
                    else
                    {
                        target.Count = items.CountEx();
                        target.GetComponents<ICollectionChangedListener<T>>().OnReset(target, items.Cast<T>());
                    }
                }

                return true;
            }

            public void OnDetaching(object owner, IReadOnlyMetadataContext? metadata)
            {
            }

            public void OnDetached(object owner, IReadOnlyMetadataContext? metadata) => TryGetTarget((IReadOnlyObservableCollection) owner)?.Dispose();

            public void OnDisposing(IReadOnlyObservableCollection owner, IReadOnlyMetadataContext? metadata)
            {
            }

            public void OnDisposed(IReadOnlyObservableCollection owner, IReadOnlyMetadataContext? metadata) => TryGetTarget(owner)?.Dispose();

            public void OnChanged(IReadOnlyObservableCollection owner, ILocker locker, IReadOnlyMetadataContext? metadata)
            {
                var target = TryGetTarget(owner);
                target?.GetComponents<ILockerChangedListener<IReadOnlyObservableCollection>>().OnChanged(target, locker, metadata);
            }

            private DecoratedReadOnlyObservableCollection<T>? TryGetTarget(IReadOnlyObservableCollection source)
            {
                if (_target is DecoratedReadOnlyObservableCollection<T> t)
                    return t;
                var target = (DecoratedReadOnlyObservableCollection<T>?) ((IWeakReference) _target).Target;
                if (target == null)
                {
                    source.RemoveComponent(this);
                    return null;
                }

                return target;
            }
        }
    }
}