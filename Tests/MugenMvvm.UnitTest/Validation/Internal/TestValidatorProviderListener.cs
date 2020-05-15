using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;

namespace MugenMvvm.UnitTest.Validation.Internal
{
    public class TestValidatorProviderListener : IValidatorProviderListener, IHasPriority
    {
        #region Properties

        public int Priority { get; set; }

        public Action<IValidatorProvider, IValidator, object, Type, IReadOnlyMetadataContext?>? OnValidatorCreated { get; set; }

        #endregion

        #region Implementation of interfaces

        void IValidatorProviderListener.OnValidatorCreated<TRequest>(IValidatorProvider provider, IValidator validator, in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            OnValidatorCreated?.Invoke(provider, validator, request!, typeof(TRequest), metadata);
        }

        #endregion
    }
}