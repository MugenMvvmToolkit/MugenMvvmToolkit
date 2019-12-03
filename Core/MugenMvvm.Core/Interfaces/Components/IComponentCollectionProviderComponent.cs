using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IComponentCollectionProviderComponent : IComponent<IComponentCollectionProvider>
    {
        IComponentCollection? TryGetComponentCollection(object owner, IReadOnlyMetadataContext? metadata);
    }
}