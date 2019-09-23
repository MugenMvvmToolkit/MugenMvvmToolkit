using MugenMvvm.Binding.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Core.Components
{
    public interface IBindingStateDispatcherComponent : IComponent<IBindingManager>
    {
        IReadOnlyMetadataContext? OnLifecycleChanged(IBinding binding, DataBindingLifecycleState lifecycle, IReadOnlyMetadataContext? metadata);
    }
}