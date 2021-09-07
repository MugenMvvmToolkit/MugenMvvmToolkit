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

        public ReadOnlyObservableCollection(IReadOnlyObservableCollection<T> source, int priority, bool disposeSource,
            IComponentCollectionManager? componentCollectionManager = null)
            : base(componentCollectionManager, source)
        {
            Should.NotBeNull(source, nameof(source));
            _source = source;
            _disposeSource = disposeSource;
            _decorator = new Listener(this, priority);
            source.AddComponent(_decorator);
        }

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

            _decorator = null;
            _source.RemoveComponent(decorator);
            GetComponents<IDisposableComponent<IReadOnlyObservableCollection>>().Dispose(this, null);
            this.ClearComponents();
            if (_disposeSource)
                _source.Dispose();
        }

        public IEnumerator<T> GetEnumerator() => _decorator == null ? Default.EmptyEnumerator<T>() : _source.GetEnumerator();

        public void UpdateLocker(ILocker locker) => _source.UpdateLocker(locker);

        public ActionToken Lock() => _source.Lock();

        public bool TryLock(out ActionToken lockToken) => _source.TryLock(out lockToken);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private sealed class Listener : ICollectionChangedListener<T>, ICollectionBatchUpdateListener, IDisposableComponent<IReadOnlyObservableCollection>, IDetachableComponent,
            IHasPriority
        {
            private readonly IWeakReference _targetRef;

            public Listener(ReadOnlyObservableCollection<T> target, int priority)
            {
                Priority = priority;
                _targetRef = target.ToWeakReference();
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

            public void Dispose(IReadOnlyObservableCollection owner, IReadOnlyMetadataContext? metadata) => TryGetTarget(owner)?.Dispose();

            private ReadOnlyObservableCollection<T>? TryGetTarget(IReadOnlyObservableCollection source)
            {
                var target = (ReadOnlyObservableCollection<T>?) _targetRef.Target;
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