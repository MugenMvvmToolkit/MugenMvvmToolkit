using MugenMvvm.Bindings.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Interfaces.Core.Components
{
    public interface IBindingLifecycleDispatcherComponent : IComponent<IBindingManager>
    {
        void OnLifecycleChanged(IBindingManager bindingManager, IBinding binding, BindingLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata);
    }
}