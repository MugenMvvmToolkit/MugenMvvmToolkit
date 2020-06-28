using System;
using MugenMvvm.Binding.Interfaces.Observation;
using MugenMvvm.Binding.Interfaces.Observation.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTest.Binding.Observation.Internal
{
    public class TestMemberPathObserverProviderComponent : IMemberPathObserverProviderComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; }

        public Func<object, object, Type, IReadOnlyMetadataContext?, IMemberPathObserver?>? TryGetMemberPathObserver { get; set; }

        #endregion

        #region Implementation of interfaces

        IMemberPathObserver? IMemberPathObserverProviderComponent.TryGetMemberPathObserver<TRequest>(object target, in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            return TryGetMemberPathObserver?.Invoke(target, request!, typeof(TRequest), metadata);
        }

        #endregion
    }
}