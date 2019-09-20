using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Observers.Components
{
    public interface IBindingPathObserverProviderComponent : IComponent<IBindingObserverProvider>
    {
        IBindingPathObserver? TryGetBindingPathObserver(object source, IBindingPath path, IReadOnlyMetadataContext? metadata);
    }
}