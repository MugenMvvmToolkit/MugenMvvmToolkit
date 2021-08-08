using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Models.Components;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;

namespace MugenMvvm.Validation.Components
{
    public sealed class ChildValidatorAdapter : AttachableComponentBase<IValidator>, IValidationHandlerComponent, IValidatorErrorManagerComponent,
        IDisposableComponent<IValidator>, IHasPriority
    {
        private ListInternal<IValidator> _validators;
        private readonly ValidatorListener _listener;

        public ChildValidatorAdapter()
        {
            _validators = new ListInternal<IValidator>(2);
            _listener = new ValidatorListener(this);
        }

        public bool DisposeChildValidators { get; set; }

        public int Priority { get; init; } = ValidationComponentPriority.ChildValidatorAdapter;

        public bool Add(IValidator validator)
        {
            Should.NotBeNull(validator, nameof(validator));
            lock (_listener)
            {
                if (_validators.Contains(validator))
                    return false;
                validator.AddComponent(_listener);
                _validators.Add(validator);
            }

            RaiseErrorsChanged(default, null);
            return true;
        }

        public bool Contains(IValidator validator)
        {
            Should.NotBeNull(validator, nameof(validator));
            lock (_listener)
            {
                return _validators.Contains(validator);
            }
        }

        public bool Remove(IValidator validator)
        {
            Should.NotBeNull(validator, nameof(validator));
            lock (_listener)
            {
                if (!_validators.Remove(validator))
                    return false;
                validator.RemoveComponent(_listener);
            }

            RaiseErrorsChanged(default, null);
            return true;
        }

        public void Dispose(IValidator owner, IReadOnlyMetadataContext? metadata)
        {
            if (!DisposeChildValidators)
                return;
            lock (_listener)
            {
                var items = _validators.Items;
                for (var i = 0; i < _validators.Count; i++)
                    items[i].Dispose();
            }
        }

        public Task TryValidateAsync(IValidator validator, string? member, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            ItemOrListEditor<Task> tasks;
            lock (_listener)
            {
                tasks = new ItemOrListEditor<Task>(_validators.Count);
                var items = _validators.Items;
                for (var i = 0; i < _validators.Count; i++)
                {
                    var task = items[i].ValidateAsync(member, cancellationToken, metadata);
                    if (!task.IsCompleted || task.IsFaulted || task.IsCanceled)
                        tasks.Add(task);
                }
            }

            return tasks.WhenAll();
        }

        public bool HasErrors(IValidator validator, ItemOrIReadOnlyList<string> members, object? source, IReadOnlyMetadataContext? metadata)
        {
            lock (_listener)
            {
                var items = _validators.Items;
                for (var i = 0; i < _validators.Count; i++)
                {
                    if (items[i].HasErrors(members, source, metadata))
                        return true;
                }

                return false;
            }
        }

        public void GetErrors(IValidator validator, ItemOrIReadOnlyList<string> members, ref ItemOrListEditor<ValidationErrorInfo> errors, object? source,
            IReadOnlyMetadataContext? metadata)
        {
            lock (_listener)
            {
                var items = _validators.Items;
                for (var i = 0; i < _validators.Count; i++)
                    items[i].GetErrors(members, ref errors, source, metadata);
            }
        }

        public void GetErrors(IValidator validator, ItemOrIReadOnlyList<string> members, ref ItemOrListEditor<object> errors, object? source, IReadOnlyMetadataContext? metadata)
        {
            lock (_listener)
            {
                var items = _validators.Items;
                for (var i = 0; i < _validators.Count; i++)
                    items[i].GetErrors(members, ref errors, source, metadata);
            }
        }

        public void SetErrors(IValidator validator, object source, ItemOrIReadOnlyList<ValidationErrorInfo> errors, IReadOnlyMetadataContext? metadata)
        {
        }

        public void ClearErrors(IValidator validator, ItemOrIReadOnlyList<string> members, object? source, IReadOnlyMetadataContext? metadata)
        {
        }

        private void RaiseErrorsChanged(ItemOrIReadOnlyList<string> members, IReadOnlyMetadataContext? metadata) =>
            OwnerOptional?.GetComponents<IValidatorErrorsChangedListener>().OnErrorsChanged(OwnerOptional, members, metadata);

        private void RaiseAsyncValidation(string? member, Task validationTask, IReadOnlyMetadataContext? metadata) =>
            OwnerOptional?.GetComponents<IAsyncValidationListener>().OnAsyncValidation(OwnerOptional, member, validationTask, metadata);

        private sealed class ValidatorListener : IValidatorErrorsChangedListener, IAsyncValidationListener
        {
            private readonly ChildValidatorAdapter _adapter;

            public ValidatorListener(ChildValidatorAdapter adapter)
            {
                _adapter = adapter;
            }

            public void OnAsyncValidation(IValidator validator, string? member, Task validationTask, IReadOnlyMetadataContext? metadata) =>
                _adapter.RaiseAsyncValidation(member, validationTask, metadata);

            public void OnErrorsChanged(IValidator validator, ItemOrIReadOnlyList<string> members, IReadOnlyMetadataContext? metadata) =>
                _adapter.RaiseErrorsChanged(members, metadata);
        }
    }
}