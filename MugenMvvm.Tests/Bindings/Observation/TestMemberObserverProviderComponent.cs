using System;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Interfaces.Observation.Components;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Tests.Bindings.Observation
{
    public class TestMemberObserverProviderComponent : IMemberObserverProviderComponent, IHasPriority
    {
        public Func<IObservationManager, Type, object, IReadOnlyMetadataContext?, MemberObserver>? TryGetMemberObserver { get; set; }

        public int Priority { get; set; }

        MemberObserver IMemberObserverProviderComponent.
            TryGetMemberObserver(IObservationManager observationManager, Type type, object member, IReadOnlyMetadataContext? metadata) =>
            TryGetMemberObserver?.Invoke(observationManager, type, member, metadata) ?? default;
    }
}