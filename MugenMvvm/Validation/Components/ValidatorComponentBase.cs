using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Collections;
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
    public abstract class ValidatorComponentBase<TTarget> : MultiAttachableComponentBase<IValidator>, IValidatorComponent, IHasTarget<TTarget>, IHasPriority, IHasDisposeCondition
        where TTarget : class
    {
        #region Fields

        private readonly Dictionary<string, object> _errors;

        private CancellationTokenSource? _disposeToken;
        private int _state;

        private const int DefaultState = 0;
        private const int NoDisposeState = 1;
        private const int DisposedState = 2;

        #endregion

        #region Constructors

        protected ValidatorComponentBase(TTarget target)
        {
            _errors = new Dictionary<string, object>(StringComparer.Ordinal);
            Target = target;
        }

        #endregion

        #region Properties

        public bool IsDisposed => _state == DisposedState;

        public TTarget Target { get; }

        public int Priority { get; set; }

        public bool IsDisposable
        {
            get => _state == DefaultState;
            set
            {
                if (value)
                    Interlocked.CompareExchange(ref _state, DefaultState, NoDisposeState);
                else
                    Interlocked.CompareExchange(ref _state, NoDisposeState, DefaultState);
            }
        }

        #endregion

        #region Implementation of interfaces

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _state, DisposedState, DefaultState) == DefaultState)
            {
                OnDispose();
                _disposeToken?.Cancel();
            }
        }

        public bool HasErrors(IValidator validator, string? memberName, IReadOnlyMetadataContext? metadata)
        {
            lock (_errors)
            {
                if (string.IsNullOrEmpty(memberName))
                    return _errors.Count != 0;
                return _errors.ContainsKey(memberName!);
            }
        }

        public ItemOrIReadOnlyList<object> TryGetErrors(IValidator validator, string? memberName, IReadOnlyMetadataContext? metadata = null)
        {
            lock (_errors)
            {
                if (_errors.Count == 0)
                    return default;

                if (string.IsNullOrEmpty(memberName))
                {
                    var errors = new ItemOrListEditor<object>();
                    foreach (var error in _errors)
                        errors.AddRange(ItemOrIEnumerable.FromRawValue<object>(error.Value));
                    return errors.ToItemOrList();
                }

                if (_errors.TryGetValue(memberName!, out var value))
                    return ItemOrIReadOnlyList.FromRawValue<object>(value);
            }

            return default;
        }

        public IReadOnlyDictionary<string, object> TryGetErrors(IValidator validator, IReadOnlyMetadataContext? metadata = null)
        {
            lock (_errors)
            {
                if (_errors.Count == 0)
                    return Default.ReadOnlyDictionary<string, object>();
                return new Dictionary<string, object>(_errors);
            }
        }

        public Task TryValidateAsync(IValidator validator, string? memberName = null, CancellationToken cancellationToken = default, IReadOnlyMetadataContext? metadata = null)
        {
            memberName ??= "";
            var task = ValidateAsync(memberName, cancellationToken, metadata);
            if (!task.IsCompleted)
                OnAsyncValidation(memberName, task, metadata);
            return task;
        }

        public void ClearErrors(IValidator? validator, string? memberName = null, IReadOnlyMetadataContext? metadata = null)
        {
            if (IsDisposed)
                return;
            if (string.IsNullOrEmpty(memberName))
            {
                var keys = new ItemOrListEditor<string>();
                lock (_errors)
                {
                    foreach (var error in _errors)
                        keys.Add(error.Key);
                }

                var count = keys.Count;
                for (var index = 0; index < count; index++)
                    UpdateErrors(keys[index], null, true, metadata);
            }
            else
                UpdateErrors(memberName!, null, true, metadata);
        }

        #endregion

        #region Methods

        protected abstract ValueTask<ValidationResult> GetErrorsAsync(string memberName, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata);

        protected virtual void OnErrorsChanged(string memberName, IReadOnlyMetadataContext? metadata)
        {
            foreach (var owner in Owners)
                owner.GetComponents<IValidatorListener>(metadata).OnErrorsChanged(owner, Target, memberName, metadata);
        }

        protected virtual void OnAsyncValidation(string memberName, Task validationTask, IReadOnlyMetadataContext? metadata)
        {
            foreach (var owner in Owners)
                owner.GetComponents<IValidatorListener>(metadata).OnAsyncValidation(owner, Target, memberName, validationTask, metadata);
        }

        protected virtual CancellationToken GetCancellationToken(string memberName, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            if (_disposeToken == null)
            {
                var cts = new CancellationTokenSource();
                if (Interlocked.CompareExchange(ref _disposeToken, cts, null) != null)
                    cts.Dispose();
            }

            if (cancellationToken.CanBeCanceled)
                return CancellationTokenSource.CreateLinkedTokenSource(_disposeToken.Token, cancellationToken).Token;
            return _disposeToken.Token;
        }

        protected virtual void OnDispose()
        {
        }

        protected void UpdateErrors(string memberName, object? rawValue, bool raiseNotifications, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(memberName, nameof(memberName));
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

        private async Task ValidateAsync(string memberName, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            if (IsDisposed)
                ExceptionManager.ThrowObjectDisposed(this);

            var result = await GetErrorsAsync(memberName, GetCancellationToken(memberName, cancellationToken, metadata), metadata).ConfigureAwait(false);
            if (!result.HasResult)
                return;

            IDictionary<string, object?> errors;
            if (string.IsNullOrEmpty(memberName))
            {
                if (result.SingleMemberName != null || result.Errors!.Count == 0)
                {
                    ClearErrors(null, memberName, result.Metadata);
                    if (result.SingleMemberName != null && result.RawErrors != null)
                        UpdateErrors(result.SingleMemberName, result.RawErrors, true, result.Metadata);
                    return;
                }

                errors = result.GetErrors();
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
                    UpdateErrors(result.SingleMemberName, result.RawErrors, true, result.Metadata);
                    if (result.SingleMemberName != memberName)
                        UpdateErrors(memberName, null, true, result.Metadata);
                    return;
                }

                errors = result.GetErrors();
                if (!errors.ContainsKey(memberName))
                    errors[memberName] = null;
            }

            foreach (var pair in errors)
                UpdateErrors(pair.Key, pair.Value, true, result.Metadata);
        }

        #endregion
    }
}