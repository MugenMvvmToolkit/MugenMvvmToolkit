using System;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;

namespace MugenMvvm.Tests.Validation
{
    public class TestValidationManagerListener : IValidationManagerListener, IHasPriority
    {
        public Action<IValidationManager, IValidator, ItemOrIReadOnlyList<object>, IReadOnlyMetadataContext?>? OnValidatorCreated { get; set; }

        public int Priority { get; set; }

        void IValidationManagerListener.OnValidatorCreated(IValidationManager validationManager, IValidator validator, ItemOrIReadOnlyList<object> request,
            IReadOnlyMetadataContext? metadata) =>
            OnValidatorCreated?.Invoke(validationManager, validator, request!, metadata);
    }
}