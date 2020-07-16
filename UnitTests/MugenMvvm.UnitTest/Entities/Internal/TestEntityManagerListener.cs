using System;
using MugenMvvm.Interfaces.Entities;
using MugenMvvm.Interfaces.Entities.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using Should;

namespace MugenMvvm.UnitTest.Entities.Internal
{
    public class TestEntityManagerListener : IEntityManagerListener, IHasPriority
    {
        #region Fields

        private readonly IEntityManager? _entityManager;

        #endregion

        #region Constructors

        public TestEntityManagerListener(IEntityManager? entityManager)
        {
            _entityManager = entityManager;
        }

        #endregion

        #region Properties

        public int Priority { get; set; }

        public Action<IEntityStateSnapshot, object, IReadOnlyMetadataContext?>? OnSnapshotCreated { get; set; }

        public Action<IEntityTrackingCollection, object?, IReadOnlyMetadataContext?>? OnTrackingCollectionCreated { get; set; }

        #endregion

        #region Methods

        void IEntityManagerListener.OnSnapshotCreated(IEntityManager entityManager, IEntityStateSnapshot snapshot, object entity, IReadOnlyMetadataContext? metadata)
        {
            _entityManager?.ShouldEqual(entityManager);
            OnSnapshotCreated?.Invoke(snapshot, entity, metadata);
        }

        void IEntityManagerListener.OnTrackingCollectionCreated(IEntityManager entityManager, IEntityTrackingCollection collection, object? request, IReadOnlyMetadataContext? metadata)
        {
            _entityManager?.ShouldEqual(entityManager);
            OnTrackingCollectionCreated?.Invoke(collection, request, metadata);
        }

        #endregion
    }
}