using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;
using Should;

namespace MugenMvvm.UnitTests.Validation.Internal
{
    public class TestValidatorProviderComponent : IValidatorProviderComponent, IHasPriority
    {
        private readonly IValidationManager? _validationManager;

        public TestValidatorProviderComponent(IValidationManager? validationManager = null)
        {
            _validationManager = validationManager;
        }

        public Func<object?, IReadOnlyMetadataContext?, IValidator?>? TryGetValidator { get; set; }

        public int Priority { get; set; }

        IValidator? IValidatorProviderComponent.TryGetValidator(IValidationManager validationManager, object? request, IReadOnlyMetadataContext? metadata)
        {
            _validationManager?.ShouldEqual(validationManager);
            return TryGetValidator?.Invoke(request, metadata);
        }
    }
}