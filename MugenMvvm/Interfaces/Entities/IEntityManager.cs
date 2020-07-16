using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Entities
{
    public interface IEntityManager : IComponentOwner<IEntityManager>
    {
        IEntityTrackingCollection? TryGetTrackingCollection(object? request = null, IReadOnlyMetadataContext? metadata = null);

        IEntityStateSnapshot? TryGetSnapshot(object entity, IReadOnlyMetadataContext? metadata = null);
    }
}