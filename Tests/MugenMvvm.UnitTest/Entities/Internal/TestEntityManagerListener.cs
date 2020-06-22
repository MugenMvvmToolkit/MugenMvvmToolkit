using System;
using MugenMvvm.Interfaces.Entities;
using MugenMvvm.Interfaces.Entities.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTest.Entities.Internal
{
    public class TestEntityManagerListener : IEntityManagerListener, IHasPriority
    {
        #region Properties

        public int Priority { get; set; }

        public Action<IEntityManager, IEntityStateSnapshot, object, object?, Type, IReadOnlyMetadataContext?>? OnSnapshotCreated { get; set; }

        public Action<IEntityManager, IEntityTrackingCollection, object?, Type, IReadOnlyMetadataContext?>? OnTrackingCollectionCreated { get; set; }

        #endregion

        #region Implementation of interfaces

        void IEntityManagerListener.OnSnapshotCreated<TState>(IEntityManager entityManager, IEntityStateSnapshot snapshot, object entity, in TState state, IReadOnlyMetadataContext? metadata)
        {
            OnSnapshotCreated?.Invoke(entityManager, snapshot, entity, state!, typeof(TState), metadata);
        }

        void IEntityManagerListener.OnTrackingCollectionCreated<TRequest>(IEntityManager entityManager, IEntityTrackingCollection collection, in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            OnTrackingCollectionCreated?.Invoke(entityManager, collection, request!, typeof(TRequest), metadata);
        }

        #endregion
    }
}