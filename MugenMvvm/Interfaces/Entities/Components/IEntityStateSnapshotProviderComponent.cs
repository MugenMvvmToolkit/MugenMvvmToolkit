using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Entities.Components
{
    public interface IEntityStateSnapshotProviderComponent : IComponent<IEntityManager>
    {
        IEntityStateSnapshot? TryGetSnapshot(IEntityManager entityManager, object entity, IReadOnlyMetadataContext? metadata);
    }
}