using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Resources.Components
{
    public interface IBindingResourceResolverComponent : IComponent<IBindingResourceResolver>
    {
        IBindingResource? TryGetBindingResource(string name, IReadOnlyMetadataContext? metadata);
    }
}