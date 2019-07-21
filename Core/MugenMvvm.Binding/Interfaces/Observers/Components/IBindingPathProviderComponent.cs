using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Observers.Components
{
    public interface IBindingPathProviderComponent : IComponent<IBindingObserverProvider>
    {
        IBindingPath? TryGetBindingPath(object path, IReadOnlyMetadataContext? metadata);
    }
}