using System;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Interfaces.Observation.Components
{
    public interface IMemberObserverProviderComponent : IComponent<IObservationManager>
    {
        MemberObserver TryGetMemberObserver(IObservationManager observationManager, Type type, object member, IReadOnlyMetadataContext? metadata);
    }
}