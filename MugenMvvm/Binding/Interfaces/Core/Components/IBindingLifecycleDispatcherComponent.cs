using MugenMvvm.Binding.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Core.Components
{
    public interface IBindingLifecycleDispatcherComponent : IComponent<IBindingManager>
    {
        void OnLifecycleChanged<TState>(IBindingManager bindingManager, IBinding binding, BindingLifecycleState lifecycleState, in TState state, IReadOnlyMetadataContext? metadata);
    }
}