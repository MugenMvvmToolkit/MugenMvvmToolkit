using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;

namespace MugenMvvm.UnitTest.Validation
{
    public class ValidatorProviderListener : IValidatorProviderListener, IHasPriority
    {
        #region Properties

        public int Priority { get; set; }

        public Action<IValidatorProvider, IValidator, object, Type, IReadOnlyMetadataContext?>? OnValidatorCreated { get; set; }

        public Action<IValidatorProvider, IAggregatorValidator, object, Type, IReadOnlyMetadataContext?>? OnAggregatorValidatorCreated { get; set; }

        #endregion

        #region Implementation of interfaces

        void IValidatorProviderListener.OnValidatorCreated<TRequest>(IValidatorProvider provider, IValidator validator, in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            OnValidatorCreated?.Invoke(provider, validator, request!, typeof(TRequest), metadata);
        }

        void IValidatorProviderListener.OnAggregatorValidatorCreated<TRequest>(IValidatorProvider provider, IAggregatorValidator validator, in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            OnAggregatorValidatorCreated?.Invoke(provider, validator, request!, typeof(TRequest), metadata);
        }

        #endregion
    }
}