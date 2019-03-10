using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Infrastructure.Metadata;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Metadata;

namespace MugenMvvm.Infrastructure.Validation
{
    public abstract class ValidatorBase<TTarget> : IValidator
        where TTarget : class
    {
        #region Fields

        private readonly HashSet<string> _validatingMembers;
        protected readonly Dictionary<string, IReadOnlyList<object>> Errors;
        private CancellationTokenSource? _disposeCancellationTokenSource;
        private int _state;

        private IComponentCollection<IValidatorListener>? _listeners;
        private TTarget _target;
        private Dictionary<string, CancellationTokenSource>? _validatingTasks;
        private PropertyChangedEventHandler? _weakPropertyHandler;

        private const int DisposedState = 1;

        #endregion

        #region Constructors

        protected ValidatorBase(IComponentCollection<IValidatorListener>? listeners, IObservableMetadataContext? metadata, bool hasAsyncValidation)
        {
            _listeners = listeners;
            Metadata = metadata ?? new MetadataContext();
            ValidateOnPropertyChanged = true;
            HasAsyncValidation = hasAsyncValidation;
            Errors = new Dictionary<string, IReadOnlyList<object>>(StringComparer.Ordinal);
            _validatingMembers = new HashSet<string>(StringComparer.Ordinal);
            _disposeCancellationTokenSource = new CancellationTokenSource();
        }

        protected ValidatorBase()
            : this(null, null, true)
        {
        }

        #endregion

        #region Properties

        public IComponentCollection<IValidatorListener> Listeners
        {
            get
            {
                if (_listeners == null)
                    _listeners = Service<IComponentCollectionFactory>.Instance.GetComponentCollection<IValidatorListener>(this, Default.MetadataContext);
                return _listeners;
            }
        }

        public bool HasErrors => !IsDisposed && HasErrorsInternal();

        object IValidator.Target => Target;

        public IObservableMetadataContext Metadata { get; }

        public bool ValidateOnPropertyChanged { get; set; }

        public bool IsDisposed { get; set; }

        public bool HasAsyncValidation { get; set; }

        protected TTarget Target => _target;

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
            this.RemoveAllListeners();
            Metadata.RemoveAllListeners();
            Metadata.Clear();
        }

        public IReadOnlyList<object> GetErrors(string? memberName)
        {
            EnsureInitialized();
            return GetErrorsInternal(memberName ?? "");
        }

        public IReadOnlyDictionary<string, IReadOnlyList<object>> GetErrors()
        {
            EnsureInitialized();
            return GetErrorsInternal();
        }

        public Task ValidateAsync(string? memberName = null, CancellationToken cancellationToken = default)
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
                    return ValidateAsyncImplAsync(member, cancellationToken);
                return ValidateInternalAsync(member, cancellationToken);
            }
            catch (Exception e)
            {
                return MugenExtensions.TaskFromException<object>(e);
            }
            finally
            {
                lock (_validatingMembers)
                {
                    _validatingMembers.Remove(member);
                }
            }
        }

        public void ClearErrors(string? memberName)
        {
            if (IsDisposed)
                return;
            EnsureInitialized();
            ClearErrorsInternal(memberName ?? "");
        }

        #endregion

        #region Methods

        public void Initialize(TTarget target, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(target, nameof(target));
            Should.NotBeNull(metadata, nameof(metadata));
            if (!MugenExtensions.LazyInitialize(ref _target, target))
                throw ExceptionManager.ObjectInitialized(GetType().Name, this);
            Metadata.Merge(metadata);
            if (ValidateOnPropertyChanged && target is INotifyPropertyChanged notifyPropertyChanged)
            {
                _weakPropertyHandler = MugenExtensions.MakeWeakPropertyChangedHandler(this, (@this, o, arg3) => @this.OnTargetPropertyChanged(arg3));
                notifyPropertyChanged.PropertyChanged += _weakPropertyHandler;
            }

            OnInitialized();
        }

        protected abstract Task<IDictionary<string, IReadOnlyList<object>?>?> GetErrorsAsync(string memberName, CancellationToken cancellationToken);

        protected virtual void OnInitialized()
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

        protected virtual IReadOnlyList<object> GetErrorsInternal(string memberName)
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

        protected virtual IReadOnlyDictionary<string, IReadOnlyList<object>> GetErrorsInternal()
        {
            lock (Errors)
            {
                return new Dictionary<string, IReadOnlyList<object>>(Errors);
            }
        }

        protected virtual Task ValidateInternalAsync(string memberName, CancellationToken cancellationToken)
        {
            var task = GetErrorsAsync(memberName, cancellationToken);
            if (task.IsCompleted)
                OnValidationCompleted(memberName, task.Result);
            else
                task.ContinueWith(t => OnValidationCompleted(memberName, t.Result), cancellationToken, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current);
            return task;
        }

        protected virtual void ClearErrorsInternal(string memberName)
        {
            if (string.IsNullOrEmpty(memberName))
            {
                string[] keys;
                lock (Errors)
                {
                    keys = Errors.Keys.ToArray();
                }

                for (var index = 0; index < keys.Length; index++)
                    UpdateErrors(keys[index], null, true);
            }
            else
                UpdateErrors(memberName, null, true);
        }

        protected virtual bool ShouldIgnoreMember(string memberName)
        {
            var collection = Metadata.Get(ValidationMetadata.IgnoredMembers);
            if (collection == null)
                return false;
            return collection.Contains(memberName);
        }

        protected virtual void OnErrorsChanged(string memberName)
        {
            var listeners = GetListeners();
            for (var i = 0; i < listeners.Count; i++)
                listeners[i].OnErrorsChanged(this, memberName);
        }

        protected virtual void OnAsyncValidation(string memberName, Task validationTask)
        {
            var listeners = GetListeners();
            for (var i = 0; i < listeners.Count; i++)
                listeners[i].OnAsyncValidation(this, memberName, validationTask);
        }

        protected virtual void OnDispose()
        {
        }

        protected void UpdateErrors(string memberName, IReadOnlyList<object>? errors, bool raiseNotifications)
        {
            Should.NotBeNull(memberName, nameof(memberName));
            var hasErrors = errors != null && errors.Count != 0;
            if (hasErrors && ShouldIgnoreMember(memberName))
                return;
            lock (Errors)
            {
                if (hasErrors)
                    Errors[memberName] = errors;
                else
                    Errors.Remove(memberName);
            }

            if (raiseNotifications)
                OnErrorsChanged(memberName);
        }

        protected IReadOnlyList<IValidatorListener> GetListeners()
        {
            return _listeners?.GetItems() ?? Default.EmptyArray<IValidatorListener>();
        }

        private void OnValidationCompleted(string memberName, IDictionary<string, IReadOnlyList<object>?>? errors)
        {
            if (errors == null)
                errors = new Dictionary<string, IReadOnlyList<object>?>();
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
                UpdateErrors(pair.Key, pair.Value, true);
        }

        private void EnsureInitialized()
        {
            if (Target == null)
                throw ExceptionManager.ObjectNotInitialized(this);
        }

        private Task ValidateAsyncImplAsync(string member, CancellationToken cancellationToken)
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

            var task = ValidateInternalAsync(member, source.Token);
            if (task.IsCompleted)
                source.Dispose();
            else
            {
                if (_validatingTasks == null)
                    MugenExtensions.LazyInitialize(ref _validatingTasks, new Dictionary<string, CancellationTokenSource>(StringComparer.Ordinal));
                CancellationTokenSource? oldValue;
                lock (_validatingTasks)
                {
                    if (_validatingTasks.TryGetValue(member, out oldValue))
                        _validatingTasks[member] = source;
                }

                oldValue?.Cancel();
                OnAsyncValidation(member, task);
                // ReSharper disable once MethodSupportsCancellation
                task.ContinueWith(t => OnAsyncValidationCompleted(member, source));
            }

            return task;
        }

        private void OnAsyncValidationCompleted(string member, CancellationTokenSource cts)
        {
            bool notify;
            lock (_validatingTasks)
            {
                notify = _validatingTasks.TryGetValue(member, out var value) && ReferenceEquals(cts, value) && _validatingTasks.Remove(member);
            }

            if (notify)
                OnErrorsChanged(member);
        }

        private void OnTargetPropertyChanged(PropertyChangedEventArgs args)
        {
            if (ValidateOnPropertyChanged)
                ValidateAsync(args.PropertyName);
        }

        #endregion
    }
}