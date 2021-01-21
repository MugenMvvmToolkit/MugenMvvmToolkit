using System;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;
using MugenMvvm.Validation;
using Should;

namespace MugenMvvm.UnitTests.Validation.Internal
{
    public class TestValidatorErrorManagerComponent : IValidatorErrorManagerComponent, IHasPriority
    {
        private readonly IValidator? _validator;

        public TestValidatorErrorManagerComponent(IValidator? validator)
        {
            _validator = validator;
        }

        public Func<ItemOrIReadOnlyList<string>, object?, IReadOnlyMetadataContext?, bool>? HasErrors { get; set; }

        public GetErrorsDelegate<ValidationErrorInfo>? GetErrors { get; set; }

        public GetErrorsDelegate<object>? GetErrorsRaw { get; set; }

        public Action<object, ItemOrIReadOnlyList<ValidationErrorInfo>, IReadOnlyMetadataContext?>? SetErrors { get; set; }

        public Action<object, ItemOrIReadOnlyList<ValidationErrorInfo>, IReadOnlyMetadataContext?>? ResetErrors { get; set; }

        public Action<ItemOrIReadOnlyList<string>, object?, IReadOnlyMetadataContext?>? ClearErrors { get; set; }

        public int Priority { get; set; }

        void IValidatorErrorManagerComponent.GetErrors(IValidator validator, ItemOrIReadOnlyList<string> members, ref ItemOrListEditor<ValidationErrorInfo> errors, object? source,
            IReadOnlyMetadataContext? metadata)
        {
            _validator?.ShouldEqual(validator);
            GetErrors?.Invoke(members, ref errors, source, metadata);
        }

        void IValidatorErrorManagerComponent.GetErrors(IValidator validator, ItemOrIReadOnlyList<string> members, ref ItemOrListEditor<object> errors, object? source,
            IReadOnlyMetadataContext? metadata)
        {
            _validator?.ShouldEqual(validator);
            GetErrorsRaw?.Invoke(members, ref errors, source, metadata);
        }

        void IValidatorErrorManagerComponent.SetErrors(IValidator validator, object source, ItemOrIReadOnlyList<ValidationErrorInfo> errors, IReadOnlyMetadataContext? metadata)
        {
            _validator?.ShouldEqual(validator);
            SetErrors?.Invoke(source, errors, metadata);
        }

        void IValidatorErrorManagerComponent.ResetErrors(IValidator validator, object source, ItemOrIReadOnlyList<ValidationErrorInfo> errors, IReadOnlyMetadataContext? metadata)
        {
            _validator?.ShouldEqual(validator);
            ResetErrors?.Invoke(source, errors, metadata);
        }

        void IValidatorErrorManagerComponent.ClearErrors(IValidator validator, ItemOrIReadOnlyList<string> members, object? source, IReadOnlyMetadataContext? metadata)
        {
            _validator?.ShouldEqual(validator);
            ClearErrors?.Invoke(members, source, metadata);
        }

        bool IValidatorErrorManagerComponent.HasErrors(IValidator validator, ItemOrIReadOnlyList<string> members, object? source, IReadOnlyMetadataContext? metadata)
        {
            _validator?.ShouldEqual(validator);
            return HasErrors?.Invoke(members, source, metadata) ?? false;
        }

        public delegate void GetErrorsDelegate<T>(ItemOrIReadOnlyList<string> members, ref ItemOrListEditor<T> errors, object? source, IReadOnlyMetadataContext? metadata);
    }
}