using System;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Interfaces.Observation.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using Should;

namespace MugenMvvm.UnitTests.Bindings.Observation.Internal
{
    public class TestMemberPathObserverProviderComponent : IMemberPathObserverProviderComponent, IHasPriority
    {
        private readonly IObservationManager? _observationManager;

        public TestMemberPathObserverProviderComponent(IObservationManager? observationManager = null)
        {
            _observationManager = observationManager;
        }

        public Func<object, object, IReadOnlyMetadataContext?, IMemberPathObserver?>? TryGetMemberPathObserver { get; set; }

        public int Priority { get; set; }

        IMemberPathObserver? IMemberPathObserverProviderComponent.TryGetMemberPathObserver(IObservationManager observationManager, object target, object request,
            IReadOnlyMetadataContext? metadata)
        {
            _observationManager?.ShouldEqual(observationManager);
            return TryGetMemberPathObserver?.Invoke(target, request, metadata);
        }
    }
}