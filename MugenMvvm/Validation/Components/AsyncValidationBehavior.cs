using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;

namespace MugenMvvm.Validation.Components
{
    public sealed class AsyncValidationBehavior : ComponentDecoratorBase<IValidator, IValidationHandlerComponent>, IValidationHandlerComponent, IValidatorErrorManagerComponent,
        IComponentCollectionDecorator<IValidatorErrorManagerComponent>
    {
        private readonly Dictionary<string, CancellationTokenSource> _validatingTasks;

        private ItemOrArray<IValidatorErrorManagerComponent> _errorManagerComponents;

        public AsyncValidationBehavior(int priority = ValidationComponentPriority.AsyncBehaviorDecorator) : base(priority)
        {
            _validatingTasks = new Dictionary<string, CancellationTokenSource>(StringComparer.Ordinal);
        }

        public void Decorate(IComponentCollection collection, ref ItemOrListEditor<IValidatorErrorManagerComponent> components, IReadOnlyMetadataContext? metadata) =>
            _errorManagerComponents = this.Decorate(ref components);

        public Task TryValidateAsync(IValidator validator, string? member, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            member ??= "";
            var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, default);
            CancellationTokenSource? oldValue;
            lock (_validatingTasks)
            {
                _validatingTasks.Remove(member, out oldValue);
            }

            oldValue.SafeCancel();

            var task = Components.TryValidateAsync(validator, member, source.Token, metadata);
            if (!task.IsCompleted)
            {
                lock (_validatingTasks)
                {
                    _validatingTasks.TryGetValue(member, out oldValue);
                    _validatingTasks[member] = source;
                }

                oldValue.SafeCancel();
                validator.GetComponents<IValidatorAsyncValidationListener>().OnAsyncValidation(validator, member, task, metadata);

                task.ContinueWith((_, s) =>
                {
                    var state = (Tuple<AsyncValidationBehavior, string, CancellationTokenSource, IReadOnlyMetadataContext?>)s!;
                    state.Item1.OnAsyncValidationCompleted(state.Item2, state.Item3, state.Item4);
                    state.Item3.Dispose();
                }, Tuple.Create(this, member, source, metadata), TaskContinuationOptions.ExecuteSynchronously);
            }

            return task;
        }

        public bool HasErrors(IValidator validator, ItemOrIReadOnlyList<string> members, object? source, IReadOnlyMetadataContext? metadata) =>
            _validatingTasks.Count != 0 || _errorManagerComponents.HasErrors(validator, members, source, metadata);

        public void GetErrors(IValidator validator, ItemOrIReadOnlyList<string> members, ref ItemOrListEditor<ValidationErrorInfo> errors, object? source,
            IReadOnlyMetadataContext? metadata) =>
            _errorManagerComponents.GetErrors(validator, members, ref errors, source, metadata);

        public void GetErrors(IValidator validator, ItemOrIReadOnlyList<string> members, ref ItemOrListEditor<object> errors, object? source, IReadOnlyMetadataContext? metadata) =>
            _errorManagerComponents.GetErrors(validator, members, ref errors, source, metadata);

        public void SetErrors(IValidator validator, object source, ItemOrIReadOnlyList<ValidationErrorInfo> errors, IReadOnlyMetadataContext? metadata) =>
            _errorManagerComponents.SetErrors(validator, source, errors, metadata);

        public void ClearErrors(IValidator validator, ItemOrIReadOnlyList<string> members, object? source, IReadOnlyMetadataContext? metadata) =>
            _errorManagerComponents.ClearErrors(validator, members, source, metadata);

        private void OnAsyncValidationCompleted(string member, CancellationTokenSource cts, IReadOnlyMetadataContext? metadata)
        {
            bool notify;
            lock (_validatingTasks)
            {
                notify = _validatingTasks.Remove(member, out var value) && cts == value;
            }

            if (notify)
                OwnerOptional?.GetComponents<IValidatorErrorsChangedListener>(metadata).OnErrorsChanged(Owner, member == "" ? default : member, metadata);
        }
    }
}