using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Observers.Components
{
    public interface IBindingPathProviderComponent : IComponent<IBindingObserverProvider>
    {
    }

    public interface IBindingPathProviderComponent<TPath> : IBindingPathProviderComponent
    {
        IBindingPath? TryGetBindingPath(in TPath path, IReadOnlyMetadataContext? metadata);
    }
}