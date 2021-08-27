using System.Collections.Generic;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Collections.Components
{
    public sealed class CollectionBatchUpdateManager : IAttachableComponent, IDetachableComponent, ICollectionBatchUpdateManagerComponent, IComponentCollectionChangingListener,
        IHasPriority
    {
        private readonly Dictionary<BatchUpdateType, int> _counters;

        public CollectionBatchUpdateManager()
        {
            _counters = new Dictionary<BatchUpdateType, int>(3);
        }

        public int Priority { get; init; } = CollectionComponentPriority.BatchUpdateManager;

        public bool IsInBatch(IReadOnlyObservableCollection collection, BatchUpdateType batchUpdateType)
        {
            using var _ = collection.Lock();
            return _counters.ContainsKey(batchUpdateType);
        }

        public void BeginBatchUpdate(IReadOnlyObservableCollection collection, BatchUpdateType batchUpdateType)
        {
            using var _ = collection.Lock();
            if (!_counters.TryGetValue(batchUpdateType, out var value))
                collection.GetComponents<ICollectionBatchUpdateListener>().OnBeginBatchUpdate(collection, batchUpdateType);

            _counters[batchUpdateType] = value + 1;
        }

        public void EndBatchUpdate(IReadOnlyObservableCollection collection, BatchUpdateType batchUpdateType)
        {
            using var _ = collection.Lock();
            Should.BeValid(_counters.TryGetValue(batchUpdateType, out var value), "BeginBatch < 0");
            if (value != 1)
            {
                _counters[batchUpdateType] = value - 1;
                return;
            }

            _counters.Remove(batchUpdateType);
            collection.GetComponents<ICollectionBatchUpdateListener>().OnEndBatchUpdate(collection, batchUpdateType);
        }

        void IAttachableComponent.OnAttaching(object owner, IReadOnlyMetadataContext? metadata)
        {
            if (owner is IReadOnlyObservableCollection collection)
                collection.Components.AddComponent(this);
        }

        void IAttachableComponent.OnAttached(object owner, IReadOnlyMetadataContext? metadata)
        {
        }

        void IComponentCollectionChangingListener.OnAdding(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (component is not ICollectionBatchUpdateListener batchUpdateListener)
                return;

            using var _ = ((IReadOnlyObservableCollection)collection.Owner).Lock();
            if (_counters.Count == 0)
                return;
            foreach (var counter in _counters)
                batchUpdateListener.OnBeginBatchUpdate((IReadOnlyObservableCollection)collection.Owner, counter.Key);
        }

        void IComponentCollectionChangingListener.OnRemoving(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (component is not ICollectionBatchUpdateListener batchUpdateListener)
                return;

            using var _ = ((IReadOnlyObservableCollection)collection.Owner).Lock();
            if (_counters.Count == 0)
                return;
            foreach (var counter in _counters)
                batchUpdateListener.OnEndBatchUpdate((IReadOnlyObservableCollection)collection.Owner, counter.Key);
        }


        void IDetachableComponent.OnDetaching(object owner, IReadOnlyMetadataContext? metadata)
        {
            if (owner is IReadOnlyObservableCollection collection)
            {
                collection.Components.RemoveComponent(this);
                using var _ = collection.Lock();
                if (_counters.Count != 0)
                {
                    var listeners = collection.GetComponents<ICollectionBatchUpdateListener>();
                    foreach (var v in _counters)
                        listeners.OnEndBatchUpdate(collection, v.Key);
                }
            }
        }

        void IDetachableComponent.OnDetached(object owner, IReadOnlyMetadataContext? metadata)
        {
        }
    }
}