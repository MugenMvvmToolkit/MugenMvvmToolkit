using System;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;

namespace MugenMvvm.Tests.Validation
{
    public class TestValidatorAsyncValidationListener : IValidatorAsyncValidationListener, IHasPriority
    {
        public Action<IValidator, string?, Task, IReadOnlyMetadataContext?>? OnAsyncValidation { get; set; }

        public int Priority { get; set; }

        void IValidatorAsyncValidationListener.OnAsyncValidation(IValidator validator, string? member, Task validationTask, IReadOnlyMetadataContext? metadata) =>
            OnAsyncValidation?.Invoke(validator, member, validationTask, metadata);
    }
}