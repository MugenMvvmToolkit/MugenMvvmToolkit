using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;
using Should;

namespace MugenMvvm.UnitTest.Validation.Internal
{
    public class TestValidatorProviderComponent : IValidatorProviderComponent, IHasPriority
    {
        #region Fields

        private readonly IValidationManager? _validationManager;

        #endregion

        #region Constructors

        public TestValidatorProviderComponent(IValidationManager? validationManager = null)
        {
            _validationManager = validationManager;
        }

        #endregion

        #region Properties

        public int Priority { get; set; }

        public Func<object?, IReadOnlyMetadataContext?, IValidator?>? TryGetValidator { get; set; }

        #endregion

        #region Implementation of interfaces

        IValidator? IValidatorProviderComponent.TryGetValidator(IValidationManager validationManager, object? request, IReadOnlyMetadataContext? metadata)
        {
            _validationManager?.ShouldEqual(validationManager);
            return TryGetValidator?.Invoke(request, metadata);
        }

        #endregion
    }
}