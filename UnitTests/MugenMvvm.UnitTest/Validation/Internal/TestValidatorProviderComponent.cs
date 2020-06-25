using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;

namespace MugenMvvm.UnitTest.Validation.Internal
{
    public class TestValidatorProviderComponent : IValidatorProviderComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; }

        public Func<object, Type?, IReadOnlyMetadataContext?, IValidator?>? TryGetValidator { get; set; }

        #endregion

        #region Implementation of interfaces

        IValidator? IValidatorProviderComponent.TryGetValidator<TRequest>(in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            return TryGetValidator?.Invoke(request!, typeof(TRequest), metadata);
        }

        #endregion
    }
}