using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Interfaces.Observation.Components
{
    public interface IMemberPathProviderComponent : IComponent<IObservationManager>
    {
        IMemberPath? TryGetMemberPath(IObservationManager observationManager, object path, IReadOnlyMetadataContext? metadata);
    }
}