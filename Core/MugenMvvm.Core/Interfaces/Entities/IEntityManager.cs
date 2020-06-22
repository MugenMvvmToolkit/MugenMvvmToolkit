using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Entities
{
    public interface IEntityManager : IComponentOwner<IEntityManager>
    {
        IEntityTrackingCollection? TryGetTrackingCollection<TRequest>(in TRequest request, IReadOnlyMetadataContext? metadata = null);

        IEntityStateSnapshot? TryGetSnapshot<TState>(object entity, in TState state, IReadOnlyMetadataContext? metadata = null);
    }
}