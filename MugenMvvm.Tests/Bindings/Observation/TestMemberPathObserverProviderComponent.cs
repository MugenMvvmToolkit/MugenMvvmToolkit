using System;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Interfaces.Observation.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Tests.Bindings.Observation
{
    public class TestMemberPathObserverProviderComponent : IMemberPathObserverProviderComponent, IHasPriority
    {
        public Func<IObservationManager, object, object, IReadOnlyMetadataContext?, IMemberPathObserver?>? TryGetMemberPathObserver { get; set; }

        public int Priority { get; set; }

        IMemberPathObserver? IMemberPathObserverProviderComponent.TryGetMemberPathObserver(IObservationManager observationManager, object target, object request,
            IReadOnlyMetadataContext? metadata) =>
            TryGetMemberPathObserver?.Invoke(observationManager, target, request, metadata);
    }
}