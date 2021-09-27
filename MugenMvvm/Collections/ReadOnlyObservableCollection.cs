using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace MugenMvvm.Collections
{
    [DebuggerDisplay("Count={" + nameof(Count) + "}")]
    [DebuggerTypeProxy(typeof(ReadOnlyObservableCollectionDebuggerProxy<>))]
    internal sealed class ReadOnlyObservableCollection<T> : ComponentOwnerBase<IReadOnlyObservableCollection>, IReadOnlyObservableCollection<T>
    {
        private readonly IReadOnlyObservableCollection<T> _source;
        private readonly bool _disposeSource;
        private Listener? _decorator;

        public ReadOnlyObservableCollection(IReadOnlyObservableCollection<T> source, int priority, bool disposeSource, bool isWeak,
            IComponentCollectionManager? componentCollectionManager = null)
            : base(componentCollectionManager, source)
        {
            Should.NotBeNull(source, nameof(source));
            _source = source;
            _disposeSource = disposeSource;
            _decorator = new Listener(this, isWeak, priority);
            source.AddComponent(_decorator);
        }

        public bool IsDisposed => _decorator == null;

        public int Count => _decorator == null ? 0 : _source.Count;

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
            if (_disposeSource)
                _source.Dispose();
        }

        public IEnumerator<T> GetEnumerator() => _decorator == null ? Default.Enumerator<T>() : _source.GetEnumerator();

        public void UpdateLocker(ILocker locker) => _source.UpdateLocker(locker);

        public ActionToken Lock() => _source.Lock();

        public bool TryLock(int timeout, out ActionToken lockToken) => _source.TryLock(timeout, out lockToken);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private sealed class Listener : ICollectionChangedListener<T>, ICollectionBatchUpdateListener, IDisposableComponent<IReadOnlyObservableCollection>, IDetachableComponent,
            ILockerChangedListener<IReadOnlyObservableCollection>, IHasPriority
        {
            private readonly bool _isWeak;
            private object? _target;

            public Listener(ReadOnlyObservableCollection<T> target, bool isWeak, int priority)
            {
                Priority = priority;
                _isWeak = isWeak;
                _target = isWeak ? target.ToWeakReference() : target;
            }

            public int Priority { get; }

            public void OnBeginBatchUpdate(IReadOnlyObservableCollection collection, BatchUpdateType batchUpdateType)
            {
                if (batchUpdateType != BatchUpdateType.Source)
                    return;
                var target = TryGetTarget(collection);
                target?.GetBatchUpdateManager().BeginBatchUpdate(target, batchUpdateType);
            }

            public void OnEndBatchUpdate(IReadOnlyObservableCollection collection, BatchUpdateType batchUpdateType)
            {
                if (batchUpdateType != BatchUpdateType.Source)
                    return;
                var target = TryGetTarget(collection);
                target?.GetBatchUpdateManager().EndBatchUpdate(target, batchUpdateType);
            }

            public void OnAdded(IReadOnlyObservableCollection<T> collection, T item, int index)
            {
                var target = TryGetTarget(collection);
                target?.GetComponents<ICollectionChangedListener<T>>().OnAdded(target, item, index);
            }

            public void OnReplaced(IReadOnlyObservableCollection<T> collection, T oldItem, T newItem, int index)
            {
                var target = TryGetTarget(collection);
                target?.GetComponents<ICollectionChangedListener<T>>().OnReplaced(target, oldItem, newItem, index);
            }

            public void OnMoved(IReadOnlyObservableCollection<T> collection, T item, int oldIndex, int newIndex)
            {
                var target = TryGetTarget(collection);
                target?.GetComponents<ICollectionChangedListener<T>>().OnMoved(target, item, oldIndex, newIndex);
            }

            public void OnRemoved(IReadOnlyObservableCollection<T> collection, T item, int index)
            {
                var target = TryGetTarget(collection);
                target?.GetComponents<ICollectionChangedListener<T>>().OnRemoved(target, item, index);
            }

            public void OnReset(IReadOnlyObservableCollection<T> collection, IEnumerable<T>? items)
            {
                var target = TryGetTarget(collection);
                target?.GetComponents<ICollectionChangedListener<T>>().OnReset(target, items);
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

            private ReadOnlyObservableCollection<T>? TryGetTarget(IReadOnlyObservableCollection source)
            {
                if (!_isWeak)
                    return (ReadOnlyObservableCollection<T>?) _target;

                var target = (ReadOnlyObservableCollection<T>?) ((IWeakReference?) _target)?.Target;
                if (target == null && _target != null)
                {
                    _target = null;
                    source.RemoveComponent(this);
                }

                return target;
            }
        }
    }
}