using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Interfaces.Observation.Components
{
    public interface IMemberPathObserverProviderComponent : IComponent<IObservationManager>
    {
        IMemberPathObserver? TryGetMemberPathObserver(IObservationManager observationManager, object target, object request, IReadOnlyMetadataContext? metadata);
    }
}