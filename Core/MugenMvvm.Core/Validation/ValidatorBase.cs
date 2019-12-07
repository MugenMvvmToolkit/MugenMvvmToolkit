using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;
using MugenMvvm.Metadata;

namespace MugenMvvm.Validation
{
    public abstract class ValidatorBase<TTarget> : ComponentOwnerBase<IValidator>, IValidator
        where TTarget : class
    {
        #region Fields

        private readonly HashSet<string> _validatingMembers;
        protected readonly Dictionary<string, IReadOnlyList<object>> Errors;
        private CancellationTokenSource? _disposeCancellationTokenSource;

        private IMetadataContext? _metadata;
        private int _state;
        private TTarget? _target;
        private Dictionary<string, CancellationTokenSource>? _validatingTasks;
        private PropertyChangedEventHandler? _weakPropertyHandler;
        private readonly IMetadataContextProvider? _metadataContextProvider;

        private const int DisposedState = 1;

        #endregion

        #region Constructors

        protected ValidatorBase(bool hasAsyncValidation, IMetadataContext? metadata = null, IComponentCollectionProvider? componentCollectionProvider = null,
            IMetadataContextProvider? metadataContextProvider = null)
            : base(componentCollectionProvider)
        {
            _metadata = metadata;
            _metadataContextProvider = metadataContextProvider;
            ValidateOnPropertyChanged = true;
            HasAsyncValidation = hasAsyncValidation;
            Errors = new Dictionary<string, IReadOnlyList<object>>(StringComparer.Ordinal);
            _validatingMembers = new HashSet<string>(StringComparer.Ordinal);
            _disposeCancellationTokenSource = new CancellationTokenSource();
        }

        #endregion

        #region Properties

        protected IMetadataContextProvider MetadataContextProvider => _metadataContextProvider.DefaultIfNull();

        public bool HasMetadata => !_metadata.IsNullOrEmpty();

        public IMetadataContext Metadata
        {
            get
            {
                if (_metadata == null)
                    _metadataContextProvider.LazyInitialize(ref _metadata, this);
                return _metadata;
            }
        }

        public bool HasErrors => !IsDisposed && HasErrorsInternal();

        public bool ValidateOnPropertyChanged { get; set; }

        public bool IsDisposed => _state == DisposedState;

        public bool HasAsyncValidation { get; set; }

        public TTarget Target => _target!;

        protected bool IsValidating => _validatingTasks != null && _validatingTasks.Count != 0;

        #endregion

        #region Implementation of interfaces

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _state, DisposedState) == DisposedState)
                return;
            OnDispose();
            _disposeCancellationTokenSource?.Cancel();
            if (Target is INotifyPropertyChanged notifyPropertyChanged && _weakPropertyHandler != null)
                notifyPropertyChanged.PropertyChanged -= _weakPropertyHandler;
            this.ClearComponents(null);
            this.ClearMetadata(true);
        }

        public IReadOnlyList<object> GetErrors(string? memberName, IReadOnlyMetadataContext? metadata = null)
        {
            EnsureInitialized();
            return GetErrorsInternal(memberName ?? "", metadata);
        }

        public IReadOnlyDictionary<string, IReadOnlyList<object>> GetErrors(IReadOnlyMetadataContext? metadata = null)
        {
            EnsureInitialized();
            return GetErrorsInternal(metadata);
        }

        public Task ValidateAsync(string? memberName = null, CancellationToken cancellationToken = default, IReadOnlyMetadataContext? metadata = null)
        {
            if (IsDisposed)
                return Default.CompletedTask;
            EnsureInitialized();
            var member = memberName ?? "";
            if (ShouldIgnoreMember(member))
                return Default.CompletedTask;
            lock (_validatingMembers)
            {
                if (!_validatingMembers.Add(member))
                    return Default.CompletedTask;
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

        public void ClearErrors(string? memberName, IReadOnlyMetadataContext? metadata = null)
        {
            if (IsDisposed)
                return;
            EnsureInitialized();
            ClearErrorsInternal(memberName ?? "", metadata);
        }

        #endregion

        #region Methods

        public void Initialize(TTarget target, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(target, nameof(target));
            if (!Extensions.MugenExtensions.LazyInitialize(ref _target, target))
                ExceptionManager.ThrowObjectInitialized(this);

            if (ValidateOnPropertyChanged && target is INotifyPropertyChanged notifyPropertyChanged)
            {
                _weakPropertyHandler = Extensions.MugenExtensions.MakeWeakPropertyChangedHandler(this, (@this, o, arg3) => @this.OnTargetPropertyChanged(arg3));
                notifyPropertyChanged.PropertyChanged += _weakPropertyHandler;
            }

            OnInitialized(metadata);
        }

        protected abstract ValueTask<ValidationResult> GetErrorsAsync(string memberName, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata);

        protected virtual void OnInitialized(IReadOnlyMetadataContext? metadata)
        {
        }

        protected virtual bool HasErrorsInternal()
        {
            if (IsValidating)
                return true;
            lock (Errors)
            {
                return Errors.Count != 0;
            }
        }

        protected virtual IReadOnlyList<object> GetErrorsInternal(string memberName, IReadOnlyMetadataContext? metadata)
        {
            lock (Errors)
            {
                if (Errors.Count == 0)
                    return Default.EmptyArray<object>();

                if (string.IsNullOrEmpty(memberName))
                {
                    var objects = new List<object>();
                    foreach (var error in Errors)
                        objects.AddRange(error.Value);
                    return objects;
                }

                if (Errors.TryGetValue(memberName, out var list))
                    return list.ToArray();
            }

            return Default.EmptyArray<object>();
        }

        protected virtual IReadOnlyDictionary<string, IReadOnlyList<object>> GetErrorsInternal(IReadOnlyMetadataContext? metadata)
        {
            lock (Errors)
            {
                if (Errors.Count == 0)
                    return Default.ReadOnlyDictionary<string, IReadOnlyList<object>>();
                return new Dictionary<string, IReadOnlyList<object>>(Errors);
            }
        }

        protected virtual Task ValidateInternalAsync(string memberName, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            var task = GetErrorsAsync(memberName, cancellationToken, metadata);
            if (task.IsCompletedSuccessfully)
            {
                OnValidationCompleted(memberName, task.Result);
                return Default.CompletedTask;
            }

            return task.AsTask().ContinueWith(t => OnValidationCompleted(memberName, t.Result), cancellationToken, TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Current);
        }

        protected virtual void ClearErrorsInternal(string memberName, IReadOnlyMetadataContext? metadata)
        {
            if (string.IsNullOrEmpty(memberName))
            {
                string[] keys;
                lock (Errors)
                {
                    keys = Errors.Keys.ToArray();
                }

                for (var index = 0; index < keys.Length; index++)
                    UpdateErrors(keys[index], null, true, metadata);
            }
            else
                UpdateErrors(memberName, null, true, metadata);
        }

        protected virtual bool ShouldIgnoreMember(string memberName)
        {
            var collection = _metadata?.Get(ValidationMetadata.IgnoredMembers);
            if (collection == null)
                return false;
            return collection.Contains(memberName);
        }

        protected virtual void OnErrorsChanged(string memberName, IReadOnlyMetadataContext? metadata)
        {
            var components = GetComponents<IValidatorListener>(metadata);
            for (var i = 0; i < components.Length; i++)
                components[i].OnErrorsChanged(this, memberName, metadata);
        }

        protected virtual void OnAsyncValidation(string memberName, Task validationTask, IReadOnlyMetadataContext? metadata)
        {
            var components = GetComponents<IValidatorListener>(metadata);
            for (var i = 0; i < components.Length; i++)
                components[i].OnAsyncValidation(this, memberName, validationTask, metadata);
        }

        protected virtual void OnDispose()
        {
            var components = GetComponents<IValidatorListener>(null);
            for (var i = 0; i < components.Length; i++)
                components[i].OnDisposed(this);
        }

        protected void UpdateErrors(string memberName, IReadOnlyList<object>? errors, bool raiseNotifications, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(memberName, nameof(memberName));
            var hasErrors = errors != null && errors.Count != 0;
            if (hasErrors && ShouldIgnoreMember(memberName))
                return;

            lock (Errors)
            {
                if (hasErrors)
                    Errors[memberName] = errors!;
                else
                    Errors.Remove(memberName);
            }

            if (raiseNotifications)
                OnErrorsChanged(memberName, metadata);
        }

        private void OnValidationCompleted(string memberName, ValidationResult result)
        {
            if (!result.HasResult)
                return;

            var errors = result.GetErrors() ?? new Dictionary<string, IReadOnlyList<object>?>();
            if (errors.IsReadOnly)
                errors = new Dictionary<string, IReadOnlyList<object>?>(errors);

            if (string.IsNullOrEmpty(memberName))
            {
                lock (Errors)
                {
                    foreach (var pair in Errors)
                    {
                        if (!errors.ContainsKey(pair.Key))
                            errors[pair.Key] = null;
                    }
                }
            }
            else
            {
                if (!errors.ContainsKey(memberName))
                    errors[memberName] = null;
            }

            foreach (var pair in errors)
                UpdateErrors(pair.Key, pair.Value, true, result.Metadata);
        }

        private void EnsureInitialized()
        {
            if (Target == null)
                ExceptionManager.ThrowObjectNotInitialized(this);
        }

        private Task ValidateAsyncImplAsync(string member, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            if (_disposeCancellationTokenSource == null)
                Extensions.MugenExtensions.LazyInitializeDisposable(ref _disposeCancellationTokenSource, new CancellationTokenSource());

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
                    Extensions.MugenExtensions.LazyInitialize(ref _validatingTasks, new Dictionary<string, CancellationTokenSource>(StringComparer.Ordinal));
                CancellationTokenSource? oldValue;
                lock (_validatingTasks)
                {
                    if (_validatingTasks.TryGetValue(member, out oldValue))
                        _validatingTasks[member] = source;
                }

                oldValue?.Cancel();
                OnAsyncValidation(member, task, metadata);
                // ReSharper disable once MethodSupportsCancellation
                task.ContinueWith(t => OnAsyncValidationCompleted(member, source, metadata));
            }

            return task;
        }

        private void OnAsyncValidationCompleted(string member, CancellationTokenSource cts, IReadOnlyMetadataContext? metadata)
        {
            bool notify;
            lock (_validatingTasks!)
            {
                notify = _validatingTasks.TryGetValue(member, out var value) && ReferenceEquals(cts, value) && _validatingTasks.Remove(member);
            }

            if (notify)
                OnErrorsChanged(member, metadata);
        }

        private void OnTargetPropertyChanged(PropertyChangedEventArgs args)
        {
            if (ValidateOnPropertyChanged)
                ValidateAsync(args.PropertyName);
        }

        #endregion

        #region Nested types

        [StructLayout(LayoutKind.Auto)]
        protected readonly struct ValidationResult
        {
            #region Fields

            public static readonly ValidationResult DoNothing = default;
            public static readonly ValidationResult Empty = new ValidationResult(Default.ReadOnlyDictionary<string, IReadOnlyList<object>?>());
            public static readonly ValueTask<ValidationResult> DoNothingTask = new ValueTask<ValidationResult>(DoNothing);
            public static readonly ValueTask<ValidationResult> EmptyTask = new ValueTask<ValidationResult>(Empty);

            public readonly IReadOnlyDictionary<string, IReadOnlyList<object>?>? ErrorsRaw;
            public readonly IReadOnlyMetadataContext Metadata;

            #endregion

            #region Constructors

            public ValidationResult(IReadOnlyDictionary<string, IReadOnlyList<object>?> errors, IReadOnlyMetadataContext? metadata = null)
            {
                Should.NotBeNull(errors, nameof(errors));
                ErrorsRaw = errors;
                Metadata = metadata.DefaultIfNull();
            }

            #endregion

            #region Properties

            public bool HasResult => ErrorsRaw != null;

            #endregion

            #region Methods

            public IDictionary<string, IReadOnlyList<object>?> GetErrors()
            {
                if (ErrorsRaw == null)
                    return new Dictionary<string, IReadOnlyList<object>?>();
                if (ErrorsRaw is IDictionary<string, IReadOnlyList<object>?> errors && !errors.IsReadOnly)
                    return errors;
                var result = new Dictionary<string, IReadOnlyList<object>?>(ErrorsRaw.Count);
                foreach (var pair in ErrorsRaw)
                    result[pair.Key] = pair.Value;
                return result;
            }

            #endregion
        }

        #endregion
    }
}