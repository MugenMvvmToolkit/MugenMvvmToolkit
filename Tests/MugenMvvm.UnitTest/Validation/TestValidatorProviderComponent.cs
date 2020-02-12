using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;

namespace MugenMvvm.UnitTest.Validation
{
    public class TestValidatorProviderComponent : IValidatorProviderComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; }

        public Func<object, Type?, IReadOnlyMetadataContext?, IReadOnlyList<IValidator>?>? TryGetValidators { get; set; }

        #endregion

        #region Implementation of interfaces

        IReadOnlyList<IValidator>? IValidatorProviderComponent.TryGetValidators<TRequest>(in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            return TryGetValidators?.Invoke(request!, typeof(TRequest), metadata);
        }

        #endregion
    }
}