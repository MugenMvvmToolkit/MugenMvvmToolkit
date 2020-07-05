using System;
using MugenMvvm.Binding.Interfaces.Observation;
using MugenMvvm.Binding.Interfaces.Observation.Components;
using MugenMvvm.Binding.Observation;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using Should;

namespace MugenMvvm.UnitTest.Binding.Observation.Internal
{
    public class TestMemberObserverProviderComponent : IMemberObserverProviderComponent, IHasPriority
    {
        #region Fields

        private readonly IObservationManager? _observationManager;

        #endregion

        #region Constructors

        public TestMemberObserverProviderComponent(IObservationManager? observationManager = null)
        {
            _observationManager = observationManager;
        }

        #endregion

        #region Properties

        public int Priority { get; set; }

        public Func<Type, object, Type, IReadOnlyMetadataContext?, MemberObserver>? TryGetMemberObserver { get; set; }

        #endregion

        #region Methods

        MemberObserver IMemberObserverProviderComponent.TryGetMemberObserver<TMember>(IObservationManager observationManager, Type type, in TMember member, IReadOnlyMetadataContext? metadata)
        {
            _observationManager?.ShouldEqual(observationManager);
            return TryGetMemberObserver?.Invoke(type, member!, typeof(TMember), metadata) ?? default;
        }

        #endregion
    }
}