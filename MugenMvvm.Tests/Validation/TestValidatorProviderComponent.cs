using System;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;

namespace MugenMvvm.Tests.Validation
{
    public class TestValidatorProviderComponent : IValidatorProviderComponent, IHasPriority
    {
        public Func<IValidationManager, ItemOrIReadOnlyList<object>, IReadOnlyMetadataContext?, IValidator?>? TryGetValidator { get; set; }

        public int Priority { get; set; }

        IValidator? IValidatorProviderComponent.TryGetValidator(IValidationManager validationManager, ItemOrIReadOnlyList<object> targets, IReadOnlyMetadataContext? metadata) =>
            TryGetValidator?.Invoke(validationManager, targets, metadata);
    }
}