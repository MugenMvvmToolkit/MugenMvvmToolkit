using System;
using MugenMvvm.Binding.Interfaces.Observation;
using MugenMvvm.Binding.Interfaces.Observation.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using Should;

namespace MugenMvvm.UnitTests.Binding.Observation.Internal
{
    public class TestMemberPathObserverProviderComponent : IMemberPathObserverProviderComponent, IHasPriority
    {
        #region Fields

        private readonly IObservationManager? _observationManager;

        #endregion

        #region Constructors

        public TestMemberPathObserverProviderComponent(IObservationManager? observationManager = null)
        {
            _observationManager = observationManager;
        }

        #endregion

        #region Properties

        public int Priority { get; set; }

        public Func<object, object, IReadOnlyMetadataContext?, IMemberPathObserver?>? TryGetMemberPathObserver { get; set; }

        #endregion

        #region Implementation of interfaces

        IMemberPathObserver? IMemberPathObserverProviderComponent.TryGetMemberPathObserver(IObservationManager observationManager, object target, object request, IReadOnlyMetadataContext? metadata)
        {
            _observationManager?.ShouldEqual(observationManager);
            return TryGetMemberPathObserver?.Invoke(target, request, metadata);
        }

        #endregion
    }
}