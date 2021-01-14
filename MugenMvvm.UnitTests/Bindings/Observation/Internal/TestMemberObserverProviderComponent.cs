using System;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Interfaces.Observation.Components;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using Should;

namespace MugenMvvm.UnitTests.Bindings.Observation.Internal
{
    public class TestMemberObserverProviderComponent : IMemberObserverProviderComponent, IHasPriority
    {
        private readonly IObservationManager? _observationManager;

        public TestMemberObserverProviderComponent(IObservationManager? observationManager = null)
        {
            _observationManager = observationManager;
        }

        public Func<Type, object, IReadOnlyMetadataContext?, MemberObserver>? TryGetMemberObserver { get; set; }

        public int Priority { get; set; }

        MemberObserver IMemberObserverProviderComponent.TryGetMemberObserver(IObservationManager observationManager, Type type, object member, IReadOnlyMetadataContext? metadata)
        {
            _observationManager?.ShouldEqual(observationManager);
            return TryGetMemberObserver?.Invoke(type, member, metadata) ?? default;
        }
    }
}