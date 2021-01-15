using MugenMvvm.Bindings.Resources;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Interfaces.Resources.Components
{
    public interface IResourceResolverComponent : IComponent<IResourceManager>
    {
        ResourceResolverResult TryGetResource(IResourceManager resourceManager, string name, object? state, IReadOnlyMetadataContext? metadata);
    }
}