using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Entities.Components
{
    public interface IEntityManagerListener : IComponent<IEntityManager>
    {
        void OnSnapshotCreated(IEntityManager entityManager, IEntityStateSnapshot snapshot, object entity, IReadOnlyMetadataContext? metadata);

        void OnTrackingCollectionCreated(IEntityManager entityManager, IEntityTrackingCollection collection, object? request, IReadOnlyMetadataContext? metadata);
    }
}