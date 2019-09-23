using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Observers.Components
{
    public interface IMemberPathProviderComponent : IComponent<IObserverProvider>
    {
    }

    public interface IMemberPathProviderComponent<TPath> : IMemberPathProviderComponent
    {
        IMemberPath? TryGetMemberPath(in TPath path, IReadOnlyMetadataContext? metadata);
    }
}