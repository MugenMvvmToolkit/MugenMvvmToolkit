using System.Collections.Immutable;
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
        private static readonly ImmutableHashSet<IValidator> Empty = ImmutableHashSet<IValidator>.Empty.WithComparer(Default.ReferenceEqualityComparer);

        private readonly ValidatorListener _listener;
        private ImmutableHashSet<IValidator> _validators;

        public ChildValidatorAdapter()
        {
            _listener = new ValidatorListener(this);
            _validators = Empty;
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
                if (!MugenExtensions.Add(ref _validators, validator))
                    return false;
                validator.AddComponent(_listener);
            }

            RaiseErrorsChanged(default, null);
            return true;
        }

        public bool Contains(IValidator validator)
        {
            Should.NotBeNull(validator, nameof(validator));
            return _validators.Contains(validator);
        }

        public bool Remove(IValidator validator)
        {
            Should.NotBeNull(validator, nameof(validator));
            lock (_listener)
            {
                if (!MugenExtensions.Remove(ref _validators, validator))
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
            foreach (var validator in _validators)
                validator.Dispose();
        }

        public Task TryValidateAsync(IValidator validator, string? member, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            var validators = _validators;
            var tasks = new ItemOrListEditor<Task>(validators.Count);
            foreach (var item in validators)
            {
                var task = item.ValidateAsync(member, cancellationToken, metadata);
                if (!task.IsCompletedSuccessfully())
                    tasks.Add(task);
            }

            return tasks.WhenAll();
        }

        public Task WaitAsync(IValidator validator, string? member, IReadOnlyMetadataContext? metadata)
        {
            var tasks = new ItemOrListEditor<Task>();
            foreach (var cmd in _validators)
            {
                var task = cmd.WaitAsync(member, metadata);
                if (!task.IsCompletedSuccessfully())
                    tasks.Add(task);
            }

            return tasks.WhenAll();
        }

        public bool HasErrors(IValidator validator, ItemOrIReadOnlyList<string> members, object? source, IReadOnlyMetadataContext? metadata)
        {
            foreach (var item in _validators)
            {
                if (item.HasErrors(members, source, metadata))
                    return true;
            }

            return false;
        }

        public void GetErrors(IValidator validator, ItemOrIReadOnlyList<string> members, ref ItemOrListEditor<ValidationErrorInfo> errors, object? source,
            IReadOnlyMetadataContext? metadata)
        {
            foreach (var item in _validators)
                item.GetErrors(members, ref errors, source, metadata);
        }

        public void GetErrors(IValidator validator, ItemOrIReadOnlyList<string> members, ref ItemOrListEditor<object> errors, object? source, IReadOnlyMetadataContext? metadata)
        {
            foreach (var item in _validators)
                item.GetErrors(members, ref errors, source, metadata);
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

        private sealed class ValidatorListener : IValidatorErrorsChangedListener, IValidatorAsyncValidationListener, IDisposableComponent<IValidator>
        {
            private readonly ChildValidatorAdapter _adapter;

            public ValidatorListener(ChildValidatorAdapter adapter)
            {
                _adapter = adapter;
            }

            public void OnDisposing(IValidator owner, IReadOnlyMetadataContext? metadata)
            {
            }

            public void OnDisposed(IValidator owner, IReadOnlyMetadataContext? metadata) => _adapter.Remove(owner);

            public void OnAsyncValidation(IValidator validator, string? member, Task validationTask, IReadOnlyMetadataContext? metadata) =>
                _adapter.RaiseAsyncValidation(member, validationTask, metadata);

            public void OnErrorsChanged(IValidator validator, ItemOrIReadOnlyList<string> members, IReadOnlyMetadataContext? metadata) =>
                _adapter.RaiseErrorsChanged(members, metadata);
        }
    }
}