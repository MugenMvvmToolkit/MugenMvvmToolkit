using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Entities.Components
{
    public interface IEntityTrackingCollectionProviderComponent : IComponent<IEntityManager>
    {
        IEntityTrackingCollection? TryGetTrackingCollection<TRequest>(IEntityManager entityManager, in TRequest request, IReadOnlyMetadataContext? metadata);
    }
}