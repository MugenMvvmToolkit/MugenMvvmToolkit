using System;
using MugenMvvm.Binding.Observation;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Observation.Components
{
    public interface IMemberObserverProviderComponent : IComponent<IObservationManager>
    {
        MemberObserver TryGetMemberObserver(IObservationManager observationManager, Type type, object member, IReadOnlyMetadataContext? metadata);
    }
}