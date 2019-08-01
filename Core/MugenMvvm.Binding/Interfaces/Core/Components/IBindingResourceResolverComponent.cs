using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Core.Components
{
    public interface IBindingResourceResolverComponent : IComponent<IBindingManager>
    {
        IBindingResource? TryGetBindingResource(string name, IReadOnlyMetadataContext? metadata);
    }
}