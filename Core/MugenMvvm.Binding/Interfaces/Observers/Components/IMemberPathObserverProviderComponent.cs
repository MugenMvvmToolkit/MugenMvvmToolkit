using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Observers.Components
{
    public interface IMemberPathObserverProviderComponent : IComponent<IObserverProvider>
    {
        IMemberPathObserver? TryGetMemberPathObserver<TPath>(object source, in TPath path, IReadOnlyMetadataContext? metadata);
    }
}