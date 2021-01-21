using System;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;
using Should;

namespace MugenMvvm.UnitTests.Validation.Internal
{
    public class TestValidationManagerListener : IValidationManagerListener, IHasPriority
    {
        private readonly IValidationManager? _validationManager;

        public TestValidationManagerListener(IValidationManager? validationManager = null)
        {
            _validationManager = validationManager;
        }

        public Action<IValidator, ItemOrIReadOnlyList<object>, IReadOnlyMetadataContext?>? OnValidatorCreated { get; set; }

        public int Priority { get; set; }

        void IValidationManagerListener.OnValidatorCreated(IValidationManager provider, IValidator validator, ItemOrIReadOnlyList<object> request,
            IReadOnlyMetadataContext? metadata)
        {
            _validationManager?.ShouldEqual(provider);
            OnValidatorCreated?.Invoke(validator, request!, metadata);
        }
    }
}