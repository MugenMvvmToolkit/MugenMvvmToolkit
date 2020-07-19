using System;
using MugenMvvm.Binding.Interfaces.Observation;
using MugenMvvm.Binding.Interfaces.Observation.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using Should;

namespace MugenMvvm.UnitTest.Binding.Observation.Internal
{
    public class TestMemberPathProviderComponent : IMemberPathProviderComponent, IHasPriority
    {
        #region Fields

        private readonly IObservationManager? _observationManager;

        #endregion

        #region Constructors

        public TestMemberPathProviderComponent(IObservationManager? observationManager = null)
        {
            _observationManager = observationManager;
        }

        #endregion

        #region Properties

        public int Priority { get; set; }

        public Func<object, IReadOnlyMetadataContext?, IMemberPath?>? TryGetMemberPath { get; set; }

        #endregion

        #region Implementation of interfaces

        IMemberPath? IMemberPathProviderComponent.TryGetMemberPath(IObservationManager observationManager, object path, IReadOnlyMetadataContext? metadata)
        {
            _observationManager?.ShouldEqual(observationManager);
            return TryGetMemberPath?.Invoke(path, metadata);
        }

        #endregion
    }
}