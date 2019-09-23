using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Resources.Components
{
    public interface IResourceResolverComponent : IComponent<IResourceResolver>
    {
        IResourceValue? TryGetResourceValue(string name, IReadOnlyMetadataContext? metadata);
    }
}