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

        public EntityManager(IComponentCollectionProvider? componentCollectionProvider = null)
            : base(componentCollectionProvider)
        {
        }

        #endregion

        #region Implementation of interfaces

        public IEntityTrackingCollection? TryGetTrackingCollection<TRequest>(in TRequest request, IReadOnlyMetadataContext? metadata = null)
        {
            var collection = GetComponents<IEntityTrackingCollectionProviderComponent>().TryGetTrackingCollection(request, metadata);
            if (collection != null)
                GetComponents<IEntityManagerListener>().OnTrackingCollectionCreated(this, collection, request, metadata);
            return collection;
        }

        public IEntityStateSnapshot? TryGetSnapshot<TState>(object entity, in TState state, IReadOnlyMetadataContext? metadata = null)
        {
            var snapshot = GetComponents<IEntityStateSnapshotProviderComponent>().TryGetSnapshot(entity, state, metadata);
            if (snapshot != null)
                GetComponents<IEntityManagerListener>().OnSnapshotCreated(this, snapshot, entity, state, metadata);
            return snapshot;
        }

        #endregion
    }
}