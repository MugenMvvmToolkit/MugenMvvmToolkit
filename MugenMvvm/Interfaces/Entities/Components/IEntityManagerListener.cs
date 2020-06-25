using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Entities.Components
{
    public interface IEntityManagerListener : IComponent<IEntityManager>
    {
        void OnSnapshotCreated<TState>(IEntityManager entityManager, IEntityStateSnapshot snapshot, object entity, in TState state, IReadOnlyMetadataContext? metadata);

        void OnTrackingCollectionCreated<TRequest>(IEntityManager entityManager, IEntityTrackingCollection collection, in TRequest request, IReadOnlyMetadataContext? metadata);
    }
}