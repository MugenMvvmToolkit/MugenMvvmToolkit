using System.Collections.Generic;
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
using MugenMvvm.Internal;

namespace MugenMvvm.Validation.Components
{
    public sealed class ChildValidatorAdapter : AttachableComponentBase<IValidator>, IValidationHandlerComponent, IValidatorErrorManagerComponent,
        IDisposableComponent<IValidator>, IHasPriority
    {
        private readonly ValidatorListener _listener;

        public ChildValidatorAdapter()
        {
            _listener = new ValidatorListener(this);
        }

        public bool DisposeChildValidators { get; set; }

        public int Priority { get; init; } = ValidationComponentPriority.ChildValidatorAdapter;

        public bool Add(IValidator validator)
        {
            Should.NotBeNull(validator, nameof(validator));
            if (OwnerOptional == validator)
                return false;
            lock (_listener)
            {
                if (!_listener.Add(validator))
                    return false;
                validator.AddComponent(_listener);
            }

            RaiseErrorsChanged(default, null);
            return true;
        }

        public bool Contains(IValidator validator)
        {
            Should.NotBeNull(validator, nameof(validator));
            lock (_listener)
            {
                return _listener.Contains(validator);
            }
        }

        public bool Remove(IValidator validator)
        {
            Should.NotBeNull(validator, nameof(validator));
            lock (_listener)
            {
                if (!_listener.Remove(validator))
                    return false;
                validator.RemoveComponent(_listener);
            }

            RaiseErrorsChanged(default, null);
            return true;
        }

        public void OnDisposing(IValidator owner, IReadOnlyMetadataContext? metadata)
        {
        }

        public void OnDisposed(IValidator owner, IReadOnlyMetadataContext? metadata)
        {
            if (!DisposeChildValidators)
                return;
            lock (_listener)
            {
                foreach (var validator in _listener)
                    validator.Dispose();
            }
        }

        public Task TryValidateAsync(IValidator validator, string? member, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            ItemOrListEditor<Task> tasks;
            lock (_listener)
            {
                tasks = new ItemOrListEditor<Task>(_listener.Count);
                foreach (var item in _listener)
                {
                    var task = item.ValidateAsync(member, cancellationToken, metadata);
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
                foreach (var item in _listener)
                {
                    if (item.HasErrors(members, source, metadata))
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
                foreach (var item in _listener)
                    item.GetErrors(members, ref errors, source, metadata);
            }
        }

        public void GetErrors(IValidator validator, ItemOrIReadOnlyList<string> members, ref ItemOrListEditor<object> errors, object? source, IReadOnlyMetadataContext? metadata)
        {
            lock (_listener)
            {
                foreach (var item in _listener)
                    item.GetErrors(members, ref errors, source, metadata);
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
            OwnerOptional?.GetComponents<IValidatorAsyncValidationListener>().OnAsyncValidation(OwnerOptional, member, validationTask, metadata);

        private sealed class ValidatorListener : HashSet<IValidator>, IValidatorErrorsChangedListener, IValidatorAsyncValidationListener
        {
            private readonly ChildValidatorAdapter _adapter;

            public ValidatorListener(ChildValidatorAdapter adapter)
#if NET461
                : base(InternalEqualityComparer.Reference)
#else
                : base(2, InternalEqualityComparer.Reference)
#endif
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