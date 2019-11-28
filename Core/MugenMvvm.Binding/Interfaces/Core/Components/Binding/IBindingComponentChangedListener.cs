using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Core.Components.Binding
{
    public interface IBindingComponentChangedListener : IComponent<IBinding>
    {
        void OnAdded(IBinding binding, IComponent<IBinding> component, IReadOnlyMetadataContext? metadata);

        void OnRemoved(IBinding binding, IComponent<IBinding> component, IReadOnlyMetadataContext? metadata);
    }
}