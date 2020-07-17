using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;
using Should;

namespace MugenMvvm.UnitTest.Validation.Internal
{
    public class TestValidatorProviderListener : IValidatorProviderListener, IHasPriority
    {
        #region Fields

        private readonly IValidationManager? _validationManager;

        #endregion

        #region Constructors

        public TestValidatorProviderListener(IValidationManager? validationManager = null)
        {
            _validationManager = validationManager;
        }

        #endregion

        #region Properties

        public int Priority { get; set; }

        public Action<IValidator, object?, IReadOnlyMetadataContext?>? OnValidatorCreated { get; set; }

        #endregion

        #region Implementation of interfaces

        void IValidatorProviderListener.OnValidatorCreated(IValidationManager provider, IValidator validator, object? request, IReadOnlyMetadataContext? metadata)
        {
            _validationManager?.ShouldEqual(provider);
            OnValidatorCreated?.Invoke(validator, request!, metadata);
        }

        #endregion
    }
}