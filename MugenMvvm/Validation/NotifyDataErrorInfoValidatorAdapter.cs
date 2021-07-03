using System;
using System.Collections;
using System.ComponentModel;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models.Components;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;

namespace MugenMvvm.Validation
{
    public sealed class NotifyDataErrorInfoValidatorAdapter : INotifyDataErrorInfo, IValidatorErrorsChangedListener, IDisposableComponent<IValidator>
    {
        private static readonly DataErrorsChangedEventArgs EmptyArgs = new("");
        private readonly object _owner;
        private readonly IValidator _validator;

        public NotifyDataErrorInfoValidatorAdapter(IValidator validator, object? owner = null)
        {
            Should.NotBeNull(validator, nameof(validator));
            _validator = validator;
            _owner = owner ?? this;
            validator.AddComponent(this);
        }

        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        public bool HasErrors => _validator.HasErrors();

        public IEnumerable GetErrors(string? propertyName)
        {
            var errors = new ItemOrListEditor<object>(2);
            _validator.GetErrors(propertyName, ref errors);
            return errors.AsList();
        }

        void IDisposableComponent<IValidator>.Dispose(IValidator owner, IReadOnlyMetadataContext? metadata) => ErrorsChanged = null;

        void IValidatorErrorsChangedListener.OnErrorsChanged(IValidator validator, ItemOrIReadOnlyList<string> members, IReadOnlyMetadataContext? metadata)
        {
            var eventHandler = ErrorsChanged;
            if (eventHandler == null)
                return;
            if (members.IsEmpty)
            {
                eventHandler(_owner, EmptyArgs);
                return;
            }

            foreach (var member in members)
                eventHandler.Invoke(_owner, new DataErrorsChangedEventArgs(member));
        }
    }
}