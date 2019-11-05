using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Core.Components
{
    public interface IBindingComponentChangingListener : IComponent<IBinding>
    {
        bool OnAdding(IBinding binding, IComponent<IBinding> component, IReadOnlyMetadataContext? metadata);

        bool OnRemoving(IBinding binding, IComponent<IBinding> component, IReadOnlyMetadataContext? metadata);
    }
}