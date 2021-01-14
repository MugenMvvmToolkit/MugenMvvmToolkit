using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;
using Should;

namespace MugenMvvm.UnitTests.Validation.Internal
{
    public class TestValidatorProviderListener : IValidatorProviderListener, IHasPriority
    {
        private readonly IValidationManager? _validationManager;

        public TestValidatorProviderListener(IValidationManager? validationManager = null)
        {
            _validationManager = validationManager;
        }

        public Action<IValidator, object?, IReadOnlyMetadataContext?>? OnValidatorCreated { get; set; }

        public int Priority { get; set; }

        void IValidatorProviderListener.OnValidatorCreated(IValidationManager provider, IValidator validator, object? request, IReadOnlyMetadataContext? metadata)
        {
            _validationManager?.ShouldEqual(provider);
            OnValidatorCreated?.Invoke(validator, request!, metadata);
        }
    }
}