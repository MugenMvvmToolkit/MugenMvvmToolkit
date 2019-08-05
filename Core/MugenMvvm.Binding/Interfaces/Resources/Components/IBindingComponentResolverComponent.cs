using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Resources.Components
{
    public interface IBindingComponentResolverComponent : IComponent<IBindingManager>
    {
        IComponent<IDataBinding>? TryGetComponent(string name, IReadOnlyMetadataContext? metadata);
    }
}