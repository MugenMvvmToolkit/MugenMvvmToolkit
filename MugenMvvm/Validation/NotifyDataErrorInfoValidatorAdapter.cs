using System;
using System.Collections;
using System.ComponentModel;
using System.Threading.Tasks;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;

namespace MugenMvvm.Validation
{
    public sealed class NotifyDataErrorInfoValidatorAdapter : INotifyDataErrorInfo, IValidatorListener
    {
        private readonly object? _owner;
        private readonly IValidator _validator;

        public NotifyDataErrorInfoValidatorAdapter(IValidator validator, object? owner = null)
        {
            Should.NotBeNull(validator, nameof(validator));
            _validator = validator;
            _owner = owner;
            validator.AddComponent(this);
        }

        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        public bool HasErrors => _validator.HasErrors();

        public IEnumerable GetErrors(string? propertyName) => _validator.GetErrors(propertyName ?? "").AsList();

        void IValidatorListener.OnErrorsChanged(IValidator validator, object? target, string memberName, IReadOnlyMetadataContext? metadata)
            => ErrorsChanged?.Invoke(_owner ?? this, new DataErrorsChangedEventArgs(memberName));

        void IValidatorListener.OnAsyncValidation(IValidator validator, object? target, string memberName, Task validationTask, IReadOnlyMetadataContext? metadata)
        {
        }

        void IValidatorListener.OnDisposed(IValidator validator) => ErrorsChanged = null;
    }
}