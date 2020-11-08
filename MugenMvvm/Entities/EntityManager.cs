using MugenMvvm.Components;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Entities;
using MugenMvvm.Interfaces.Entities.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Entities
{
    public sealed class EntityManager : ComponentOwnerBase<IEntityManager>, IEntityManager
    {
        #region Constructors

        public EntityManager(IComponentCollectionManager? componentCollectionManager = null)
            : base(componentCollectionManager)
        {
        }

        #endregion

        #region Implementation of interfaces

        public IEntityTrackingCollection? TryGetTrackingCollection(object? request = null, IReadOnlyMetadataContext? metadata = null)
        {
            var collection = GetComponents<IEntityTrackingCollectionProviderComponent>(metadata).TryGetTrackingCollection(this, request, metadata);
            if (collection != null)
                GetComponents<IEntityManagerListener>(metadata).OnTrackingCollectionCreated(this, collection, request, metadata);
            return collection;
        }

        public IEntityStateSnapshot? TryGetSnapshot(object entity, IReadOnlyMetadataContext? metadata = null)
        {
            var snapshot = GetComponents<IEntityStateSnapshotProviderComponent>(metadata).TryGetSnapshot(this, entity, metadata);
            if (snapshot != null)
                GetComponents<IEntityManagerListener>(metadata).OnSnapshotCreated(this, snapshot, entity, metadata);
            return snapshot;
        }

        #endregion
    }
}