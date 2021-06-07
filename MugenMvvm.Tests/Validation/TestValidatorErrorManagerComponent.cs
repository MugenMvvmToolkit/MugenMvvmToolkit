using System;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;
using MugenMvvm.Validation;

namespace MugenMvvm.Tests.Validation
{
    public class TestValidatorErrorManagerComponent : IValidatorErrorManagerComponent, IHasPriority
    {
        public Func<IValidator, ItemOrIReadOnlyList<string>, object?, IReadOnlyMetadataContext?, bool>? HasErrors { get; set; }

        public GetErrorsDelegate<ValidationErrorInfo>? GetErrors { get; set; }

        public GetErrorsDelegate<object>? GetErrorsRaw { get; set; }

        public Action<IValidator, object, ItemOrIReadOnlyList<ValidationErrorInfo>, IReadOnlyMetadataContext?>? SetErrors { get; set; }

        public Action<IValidator, ItemOrIReadOnlyList<string>, object?, IReadOnlyMetadataContext?>? ClearErrors { get; set; }

        public int Priority { get; set; }

        void IValidatorErrorManagerComponent.GetErrors(IValidator validator, ItemOrIReadOnlyList<string> members, ref ItemOrListEditor<ValidationErrorInfo> errors, object? source,
            IReadOnlyMetadataContext? metadata) =>
            GetErrors?.Invoke(validator, members, ref errors, source, metadata);

        void IValidatorErrorManagerComponent.GetErrors(IValidator validator, ItemOrIReadOnlyList<string> members, ref ItemOrListEditor<object> errors, object? source,
            IReadOnlyMetadataContext? metadata) =>
            GetErrorsRaw?.Invoke(validator, members, ref errors, source, metadata);

        void IValidatorErrorManagerComponent.SetErrors(IValidator validator, object source, ItemOrIReadOnlyList<ValidationErrorInfo> errors, IReadOnlyMetadataContext? metadata) =>
            SetErrors?.Invoke(validator, source, errors, metadata);

        void IValidatorErrorManagerComponent.ClearErrors(IValidator validator, ItemOrIReadOnlyList<string> members, object? source, IReadOnlyMetadataContext? metadata) =>
            ClearErrors?.Invoke(validator, members, source, metadata);

        bool IValidatorErrorManagerComponent.HasErrors(IValidator validator, ItemOrIReadOnlyList<string> members, object? source, IReadOnlyMetadataContext? metadata) =>
            HasErrors?.Invoke(validator, members, source, metadata) ?? false;

        public delegate void GetErrorsDelegate<T>(IValidator validator, ItemOrIReadOnlyList<string> members, ref ItemOrListEditor<T> errors, object? source,
            IReadOnlyMetadataContext? metadata);
    }
}