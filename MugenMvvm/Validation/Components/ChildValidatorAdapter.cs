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
    public sealed class ChildValidatorAdapter : MultiAttachableComponentBase<IValidator>, IValidationHandlerComponent, IValidatorErrorManagerComponent,
        IDisposableComponent<IValidator>, IHasPriority
    {
        private readonly ValidatorListener _validators;

        public ChildValidatorAdapter()
        {
            _validators = new ValidatorListener(this);
        }

        public bool DisposeChildValidators { get; set; }

        public int Priority { get; init; } = ValidationComponentPriority.ChildValidatorAdapter;

        public void Add(IValidator validator)
        {
            Should.NotBeNull(validator, nameof(validator));
            lock (_validators)
            {
                if (_validators.Contains(validator))
                    return;
                validator.AddComponent(_validators);
                _validators.Add(validator);
            }

            RaiseErrorsChanged(default, null);
        }

        public bool Contains(IValidator validator)
        {
            Should.NotBeNull(validator, nameof(validator));
            lock (_validators)
            {
                return _validators.Contains(validator);
            }
        }

        public void Remove(IValidator validator)
        {
            Should.NotBeNull(validator, nameof(validator));
            lock (_validators)
            {
                if (!_validators.Remove(validator))
                    return;
                validator.RemoveComponent(_validators);
            }

            RaiseErrorsChanged(default, null);
        }

        public void Dispose(IValidator owner, IReadOnlyMetadataContext? metadata)
        {
            if (!DisposeChildValidators)
                return;
            lock (_validators)
            {
                var items = _validators.Items;
                for (var i = 0; i < _validators.Count; i++)
                    items[i].Dispose();
            }
        }

        public Task TryValidateAsync(IValidator validator, string? member, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            ItemOrListEditor<Task> tasks;
            lock (_validators)
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
            lock (_validators)
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
            lock (_validators)
            {
                var items = _validators.Items;
                for (var i = 0; i < _validators.Count; i++)
                    items[i].GetErrors(members, ref errors, source, metadata);
            }
        }

        public void GetErrors(IValidator validator, ItemOrIReadOnlyList<string> members, ref ItemOrListEditor<object> errors, object? source, IReadOnlyMetadataContext? metadata)
        {
            lock (_validators)
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

        private void RaiseErrorsChanged(ItemOrIReadOnlyList<string> members, IReadOnlyMetadataContext? metadata)
        {
            foreach (var owner in Owners)
                owner.GetComponents<IValidatorErrorsChangedListener>().OnErrorsChanged(owner, members, metadata);
        }

        private void RaiseAsyncValidation(string? member, Task validationTask, IReadOnlyMetadataContext? metadata)
        {
            foreach (var owner in Owners)
                owner.GetComponents<IAsyncValidationListener>().OnAsyncValidation(owner, member, validationTask, metadata);
        }

        private sealed class ValidatorListener : ListInternal<IValidator>, IValidatorErrorsChangedListener, IAsyncValidationListener
        {
            private readonly ChildValidatorAdapter _adapter;

            public ValidatorListener(ChildValidatorAdapter adapter) : base(2)
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