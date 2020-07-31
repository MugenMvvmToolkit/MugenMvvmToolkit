using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Validation.Components
{
    public abstract class ValidatorComponentBase<TTarget> : AttachableComponentBase<IValidator>, IValidatorComponent, IHasTarget<TTarget>, IHasPriority, IDisposable
        where TTarget : class
    {
        #region Fields

        private readonly Dictionary<string, object> _errors;

        private readonly HashSet<string> _validatingMembers;

        private CancellationTokenSource? _disposeCancellationTokenSource;
        private int _state;
        private Dictionary<string, CancellationTokenSource>? _validatingTasks;

        private const int DisposedState = 1;

        #endregion

        #region Constructors

        protected ValidatorComponentBase(TTarget target, bool hasAsyncValidation)
        {
            HasAsyncValidation = hasAsyncValidation;
            _errors = new Dictionary<string, object>(StringComparer.Ordinal);
            _validatingMembers = new HashSet<string>(StringComparer.Ordinal);
            _disposeCancellationTokenSource = new CancellationTokenSource();
            Target = target;
        }

        #endregion

        #region Properties

        public bool IsDisposed => _state == DisposedState;

        public TTarget Target { get; }

        protected bool HasAsyncValidation { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _state, DisposedState) == DisposedState)
                return;
            OnDispose();
            _disposeCancellationTokenSource?.Cancel();
        }

        public bool HasErrors(IValidator validator, string? memberName = null, IReadOnlyMetadataContext? metadata = null)
        {
            if (_validatingTasks != null && _validatingTasks.Count != 0)
                return true;
            lock (_errors)
            {
                if (string.IsNullOrEmpty(memberName))
                    return _errors.Count != 0;
                return _errors.ContainsKey(memberName!);
            }
        }

        public ItemOrList<object, IReadOnlyList<object>> TryGetErrors(IValidator validator, string? memberName, IReadOnlyMetadataContext? metadata = null)
        {
            lock (_errors)
            {
                if (_errors.Count == 0)
                    return default;

                if (string.IsNullOrEmpty(memberName))
                {
                    ItemOrListEditor<object, List<object>> errors = ItemOrListEditor.Get<object>();
                    foreach (var error in _errors)
                        errors.AddRange(ItemOrList.FromRawValue<object, IReadOnlyList<object>>(error.Value, true));
                    return errors.ToItemOrList<IReadOnlyList<object>>();
                }

                if (_errors.TryGetValue(memberName!, out var value))
                    return ItemOrList.FromRawValue<object, IReadOnlyList<object>>(value, true);
            }

            return default;
        }

        public IReadOnlyDictionary<string, ItemOrList<object, IReadOnlyList<object>>> TryGetErrors(IValidator validator, IReadOnlyMetadataContext? metadata = null)
        {
            lock (_errors)
            {
                if (_errors.Count == 0)
                    return Default.ReadOnlyDictionary<string, ItemOrList<object, IReadOnlyList<object>>>();
                var errors = new Dictionary<string, ItemOrList<object, IReadOnlyList<object>>>();
                foreach (var error in _errors)
                    errors[error.Key] = ItemOrList.FromRawValue<object, IReadOnlyList<object>>(error.Value, true);
                return errors;
            }
        }

        public Task? TryValidateAsync(IValidator validator, string? memberName = null, CancellationToken cancellationToken = default, IReadOnlyMetadataContext? metadata = null)
        {
            if (IsDisposed)
                return null;

            var member = memberName ?? "";
            lock (_validatingMembers)
            {
                if (!_validatingMembers.Add(member))
                    return null;
            }

            try
            {
                if (HasAsyncValidation)
                    return ValidateAsyncImplAsync(member, cancellationToken, metadata);
                return ValidateInternalAsync(member, cancellationToken, metadata);
            }
            catch (Exception e)
            {
                return e.TaskFromException<object>();
            }
            finally
            {
                lock (_validatingMembers)
                {
                    _validatingMembers.Remove(member);
                }
            }
        }

        public void ClearErrors(IValidator validator, string? memberName = null, IReadOnlyMetadataContext? metadata = null)
        {
            if (IsDisposed)
                return;
            if (string.IsNullOrEmpty(memberName))
            {
                ItemOrListEditor<string, List<string>> keys = ItemOrListEditor.Get<string>();
                lock (_errors)
                {
                    foreach (var error in _errors)
                        keys.Add(error.Key);
                }

                var count = keys.Count;
                for (var index = 0; index < count; index++)
                    UpdateErrors(keys[index], default, true, metadata);
            }
            else
                UpdateErrors(memberName!, default, true, metadata);
        }

        #endregion

        #region Methods

        protected abstract ValueTask<ValidationResult> GetErrorsAsync(string memberName, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata);

        protected virtual void OnErrorsChanged(string memberName, IReadOnlyMetadataContext? metadata)
        {
            OwnerOptional?.GetComponents<IValidatorListener>(metadata).OnErrorsChanged(Owner, Target, memberName, metadata);
        }

        protected virtual void OnAsyncValidation(string memberName, Task validationTask, IReadOnlyMetadataContext? metadata)
        {
            OwnerOptional?.GetComponents<IValidatorListener>(metadata).OnAsyncValidation(Owner, Target, memberName, validationTask, metadata);
        }

        protected virtual void OnDispose()
        {
        }

        protected void UpdateErrors(string memberName, ItemOrList<object, IReadOnlyList<object>> errors, bool raiseNotifications, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(memberName, nameof(memberName));
            var rawValue = errors.GetRawValue();
            lock (_errors)
            {
                if (rawValue == null)
                    _errors.Remove(memberName);
                else
                    _errors[memberName] = rawValue;
            }

            if (raiseNotifications)
                OnErrorsChanged(memberName, metadata);
        }

        private Task ValidateInternalAsync(string memberName, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            var task = GetErrorsAsync(memberName, cancellationToken, metadata);
            if (task.IsCompletedSuccessfully)
            {
                OnValidationCompleted(memberName, task.Result);
                return Default.CompletedTask;
            }

            return task
                .AsTask()
                .ContinueWith((t, state) =>
                {
                    var tuple = (Tuple<ValidatorComponentBase<TTarget>, string>)state!;
                    tuple.Item1.OnValidationCompleted(tuple.Item2, t.Result);
                }, Tuple.Create(this, memberName), cancellationToken, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current);
        }

        private void OnValidationCompleted(string memberName, ValidationResult result)
        {
            if (!result.HasResult)
                return;

            IDictionary<string, ItemOrList<object, IReadOnlyList<object>>> errors;
            if (string.IsNullOrEmpty(memberName))
            {
                if (result.SingleMemberName != null || result.Errors!.Count == 0)
                {
                    ClearErrors(OwnerOptional!, memberName, result.Metadata);
                    if (result.SingleMemberName != null && !result.SingleMemberErrors.IsNullOrEmpty())
                        UpdateErrors(result.SingleMemberName, result.SingleMemberErrors, true, result.Metadata);
                    return;
                }

                errors = result.GetErrorsNonReadOnly();
                lock (_errors)
                {
                    foreach (var pair in _errors)
                    {
                        if (!errors.ContainsKey(pair.Key))
                            errors[pair.Key] = null;
                    }
                }
            }
            else
            {
                if (result.SingleMemberName != null)
                {
                    UpdateErrors(result.SingleMemberName, result.SingleMemberErrors, true, result.Metadata);
                    if (result.SingleMemberName != memberName)
                        UpdateErrors(memberName, default, true, result.Metadata);
                    return;
                }

                errors = result.GetErrorsNonReadOnly();
                if (!errors.ContainsKey(memberName))
                    errors[memberName] = null;
            }

            foreach (var pair in errors)
                UpdateErrors(pair.Key, pair.Value, true, result.Metadata);
        }

        private Task ValidateAsyncImplAsync(string member, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            if (_disposeCancellationTokenSource == null)
                MugenExtensions.LazyInitializeDisposable(ref _disposeCancellationTokenSource, new CancellationTokenSource());

            var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _disposeCancellationTokenSource.Token);
            if (_validatingTasks != null)
            {
                CancellationTokenSource? oldValue;
                lock (_validatingTasks)
                {
                    if (_validatingTasks.TryGetValue(member, out oldValue))
                        _validatingTasks.Remove(member);
                }

                oldValue?.Cancel();
            }

            var task = ValidateInternalAsync(member, source.Token, metadata);
            if (task.IsCompleted)
                source.Dispose();
            else
            {
                if (_validatingTasks == null)
                    MugenExtensions.LazyInitialize(ref _validatingTasks, new Dictionary<string, CancellationTokenSource>(StringComparer.Ordinal));
                CancellationTokenSource? oldValue;
                lock (_validatingTasks)
                {
                    _validatingTasks.TryGetValue(member, out oldValue);
                    _validatingTasks[member] = source;
                }

                oldValue?.Cancel();
                OnAsyncValidation(member, task, metadata);
                task.ContinueWithEx((this, member, source, metadata), (t, state) => state.Item1.OnAsyncValidationCompleted(state.member, state.source, state.metadata));
            }

            return task;
        }

        private void OnAsyncValidationCompleted(string member, CancellationTokenSource cts, IReadOnlyMetadataContext? metadata)
        {
            bool notify;
            lock (_validatingTasks!)
            {
                notify = _validatingTasks.TryGetValue(member, out var value) && cts == value && _validatingTasks.Remove(member);
            }

            if (notify)
                OnErrorsChanged(member, metadata);
        }

        #endregion
    }
}