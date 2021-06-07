using System;
using MugenMvvm.Interfaces.Entities;
using MugenMvvm.Interfaces.Entities.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Tests.Entities
{
    public class TestEntityManagerListener : IEntityManagerListener, IHasPriority
    {
        public Action<IEntityManager, IEntityStateSnapshot, object, IReadOnlyMetadataContext?>? OnSnapshotCreated { get; set; }

        public Action<IEntityManager, IEntityTrackingCollection, object?, IReadOnlyMetadataContext?>? OnTrackingCollectionCreated { get; set; }

        public int Priority { get; set; }

        void IEntityManagerListener.OnSnapshotCreated(IEntityManager entityManager, IEntityStateSnapshot snapshot, object entity, IReadOnlyMetadataContext? metadata) =>
            OnSnapshotCreated?.Invoke(entityManager, snapshot, entity, metadata);

        void IEntityManagerListener.OnTrackingCollectionCreated(IEntityManager entityManager, IEntityTrackingCollection collection, object? request,
            IReadOnlyMetadataContext? metadata) =>
            OnTrackingCollectionCreated?.Invoke(entityManager, collection, request, metadata);
    }
}