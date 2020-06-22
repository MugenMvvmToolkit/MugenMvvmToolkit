using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Entities.Components
{
    public interface IEntityStateSnapshotProviderComponent : IComponent<IEntityManager>
    {
        IEntityStateSnapshot? TryGetSnapshot<TState>(object entity, in TState state, IReadOnlyMetadataContext? metadata);
    }
}