using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Observers.Components
{
    public interface IMemberPathObserverProviderComponent : IComponent<IObserverProvider>
    {
        IMemberPathObserver? TryGetMemberPathObserver(object source, IMemberPath path, IReadOnlyMetadataContext? metadata);
    }
}