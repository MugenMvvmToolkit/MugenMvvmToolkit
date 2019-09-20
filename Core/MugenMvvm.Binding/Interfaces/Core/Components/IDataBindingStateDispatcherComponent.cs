using MugenMvvm.Binding.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Core.Components
{
    public interface IDataBindingStateDispatcherComponent : IComponent<IBindingManager>
    {
        IReadOnlyMetadataContext? OnLifecycleChanged(IDataBinding binding, DataBindingLifecycleState lifecycle, IReadOnlyMetadataContext? metadata);
    }
}