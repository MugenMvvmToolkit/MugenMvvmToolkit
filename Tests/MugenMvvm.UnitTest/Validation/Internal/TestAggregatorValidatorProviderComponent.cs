using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;

namespace MugenMvvm.UnitTest.Validation.Internal
{
    public class TestAggregatorValidatorProviderComponent : IAggregatorValidatorProviderComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; }

        public Func<object, Type?, IReadOnlyMetadataContext?, IAggregatorValidator?>? TryGetAggregatorValidator { get; set; }

        #endregion

        #region Implementation of interfaces

        IAggregatorValidator? IAggregatorValidatorProviderComponent.TryGetAggregatorValidator<TRequest>(in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            return TryGetAggregatorValidator?.Invoke(request!, typeof(TRequest), metadata);
        }

        #endregion
    }
}