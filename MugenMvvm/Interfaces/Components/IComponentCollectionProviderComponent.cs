using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IComponentCollectionProviderComponent : IComponent<IComponentCollectionManager>
    {
        IComponentCollection? TryGetComponentCollection(object owner, IReadOnlyMetadataContext? metadata);
    }
}