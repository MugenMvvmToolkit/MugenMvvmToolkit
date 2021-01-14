using System;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Interfaces.Observation.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using Should;

namespace MugenMvvm.UnitTests.Bindings.Observation.Internal
{
    public class TestMemberPathProviderComponent : IMemberPathProviderComponent, IHasPriority
    {
        private readonly IObservationManager? _observationManager;

        public TestMemberPathProviderComponent(IObservationManager? observationManager = null)
        {
            _observationManager = observationManager;
        }

        public Func<object, IReadOnlyMetadataContext?, IMemberPath?>? TryGetMemberPath { get; set; }

        public int Priority { get; set; }

        IMemberPath? IMemberPathProviderComponent.TryGetMemberPath(IObservationManager observationManager, object path, IReadOnlyMetadataContext? metadata)
        {
            _observationManager?.ShouldEqual(observationManager);
            return TryGetMemberPath?.Invoke(path, metadata);
        }
    }
}