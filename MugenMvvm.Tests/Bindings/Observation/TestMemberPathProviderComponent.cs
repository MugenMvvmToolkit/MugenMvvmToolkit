using System;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Interfaces.Observation.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Tests.Bindings.Observation
{
    public class TestMemberPathProviderComponent : IMemberPathProviderComponent, IHasPriority
    {
        public Func<IObservationManager, object, IReadOnlyMetadataContext?, IMemberPath?>? TryGetMemberPath { get; set; }

        public int Priority { get; set; }

        IMemberPath? IMemberPathProviderComponent.TryGetMemberPath(IObservationManager observationManager, object path, IReadOnlyMetadataContext? metadata) =>
            TryGetMemberPath?.Invoke(observationManager, path, metadata);
    }
}