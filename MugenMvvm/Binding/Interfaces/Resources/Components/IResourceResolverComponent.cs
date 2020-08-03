using MugenMvvm.Binding.Resources;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Resources.Components
{
    public interface IResourceResolverComponent : IComponent<IResourceResolver>
    {
        ResourceResolverResult TryGetResource(IResourceResolver resourceResolver, string name, object? state, IReadOnlyMetadataContext? metadata);
    }
}