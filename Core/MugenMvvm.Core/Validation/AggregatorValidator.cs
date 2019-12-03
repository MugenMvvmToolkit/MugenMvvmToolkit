using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Attributes;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;

namespace MugenMvvm.Validation
{
    public class AggregatorValidator : ComponentOwnerBase<IValidator>, IAggregatorValidator, IValidatorListener, IComponentOwnerAddedCallback, IComponentOwnerAddingCallback, IComponentOwnerRemovedCallback
    {
        #region Fields

        private readonly IMetadataContextProvider? _metadataContextProvider;

        private InlineValidator<object>? _inlineValidator;
        private IMetadataContext? _metadata;
        private int _state;

        private const int DisposedState = -1;
        private const int InitializedState = 1;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public AggregatorValidator(IComponentCollectionProvider? componentCollectionProvider = null, IMetadataContextProvider? metadataContextProvider = null)
            : base(componentCollectionProvider)
        {
            _metadataContextProvider = metadataContextProvider;
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

        public bool IsDisposed => _state == DisposedState;

        #endregion

        #region Implementation of interfaces

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _state, DisposedState) == DisposedState)
                return;
            OnDispose();
            this.ClearComponents();
            this.ClearMetadata(true);
        }

        public IReadOnlyList<object> GetErrors(string? memberName, IReadOnlyMetadataContext? metadata = null)
        {
            EnsureInitialized();
            return GetErrorsInternal(memberName, metadata);
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
            return ValidateInternalAsync(memberName, cancellationToken, metadata);
        }

        public void ClearErrors(string? memberName = null, IReadOnlyMetadataContext? metadata = null)
        {
            if (IsDisposed)
                return;
            EnsureInitialized();
            ClearErrorsInternal(memberName, metadata);
        }

        public void SetErrors(string memberName, IReadOnlyList<object> errors, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(memberName, nameof(memberName));
            Should.NotBeNull(errors, nameof(errors));
            if (IsDisposed)
                ExceptionManager.ThrowObjectDisposed(GetType());
            SetErrorsInternal(memberName, errors, metadata);
        }

        void IComponentOwnerAddedCallback.OnComponentAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (component is IValidator validator)
                OnValidatorAdded(validator, metadata);
        }

        bool IComponentOwnerAddingCallback.OnComponentAdding(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (ReferenceEquals(this, component))
                return false;
            if (component is IValidator validator)
                return OnValidatorAdding(validator, metadata);
            return true;
        }

        void IComponentOwnerRemovedCallback.OnComponentRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (component is IValidator validator)
                OnValidatorRemoved(validator, metadata);
        }

        void IValidatorListener.OnErrorsChanged(IValidator validator, string memberName, IReadOnlyMetadataContext? metadata)
        {
            OnErrorsChanged(validator, memberName, metadata);
        }

        void IValidatorListener.OnAsyncValidation(IValidator validator, string memberName, Task validationTask, IReadOnlyMetadataContext? metadata)
        {
            OnAsyncValidation(validator, memberName, validationTask, metadata);
        }

        void IValidatorListener.OnDisposed(IValidator validator)
        {
            OnDispose(validator);
        }

        #endregion

        #region Methods

        public void Initialize(IReadOnlyMetadataContext? metadata = null)
        {
            if (Interlocked.CompareExchange(ref _state, InitializedState, 0) != 0)
                ExceptionManager.ThrowObjectInitialized(this);
            OnInitialized(metadata);
        }

        protected virtual IReadOnlyList<object> GetErrorsInternal(string? memberName, IReadOnlyMetadataContext? metadata)
        {
            List<object>? errors = null;
            var components = GetComponents<IValidator>(metadata);
            for (var i = 0; i < components.Length; i++)
            {
                var list = components[i].GetErrors(memberName, metadata);
                if (list == null || list.Count == 0)
                    continue;

                if (errors == null)
                    errors = new List<object>();
                errors.AddRange(list);
            }

            return errors ?? (IReadOnlyList<object>)Default.EmptyArray<object>();
        }

        protected virtual IReadOnlyDictionary<string, IReadOnlyList<object>> GetErrorsInternal(IReadOnlyMetadataContext? metadata)
        {
            Dictionary<string, IReadOnlyList<object>>? errors = null;
            var components = GetComponents<IValidator>(metadata);
            for (var i = 0; i < components.Length; i++)
            {
                var dictionary = components[i].GetErrors(metadata);
                if (dictionary == null || dictionary.Count == 0)
                    continue;

                foreach (var keyValuePair in dictionary)
                {
                    if (keyValuePair.Value.Count == 0)
                        continue;

                    if (errors == null)
                        errors = new Dictionary<string, IReadOnlyList<object>>();

                    if (!errors.TryGetValue(keyValuePair.Key, out var list))
                    {
                        list = new List<object>();
                        errors[keyValuePair.Key] = list;
                    }

                    ((List<object>)list).AddRange(keyValuePair.Value);
                }
            }

            if (errors == null)
                return Default.ReadOnlyDictionary<string, IReadOnlyList<object>>();
            return errors;
        }

        protected virtual Task ValidateInternalAsync(string? memberName, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            var components = GetComponents<IValidator>(metadata);
            if (components.Length == 0)
                return Default.CompletedTask;
            if (components.Length == 1)
                return components[0].ValidateAsync(memberName, cancellationToken, metadata);

            Task[] tasks = new Task[components.Length];
            for (int i = 0; i < components.Length; i++)
                tasks[i] = components[i].ValidateAsync(memberName, cancellationToken, metadata);
            return Task.WhenAll(tasks);
        }

        protected virtual void ClearErrorsInternal(string? memberName, IReadOnlyMetadataContext? metadata)
        {
            var components = GetComponents<IValidator>(metadata);
            for (var i = 0; i < components.Length; i++)
                components[i].ClearErrors(memberName, metadata);
        }

        protected void SetErrorsInternal(string memberName, IReadOnlyList<object> errors, IReadOnlyMetadataContext? metadata = null)
        {
            if (_inlineValidator == null && MugenExtensions.LazyInitialize(ref _inlineValidator, new InlineValidator<object>()))
            {
                _inlineValidator.Initialize(this);
                _inlineValidator.AddComponent(this);
                Components.Add(_inlineValidator);
            }

            _inlineValidator.SetErrors(memberName, errors, metadata);
        }

        protected virtual bool HasErrorsInternal()
        {
            var components = GetComponents<IValidator>(null);
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].HasErrors)
                    return true;
            }

            return false;
        }

        protected virtual void OnInitialized(IReadOnlyMetadataContext? metadata)
        {
        }

        protected virtual void OnDispose()
        {
            var components = GetComponents<IValidatorListener>(null);
            for (var i = 0; i < components.Length; i++)
                components[i].OnDisposed(this);
        }

        protected virtual void OnErrorsChanged(IValidator validator, string memberName, IReadOnlyMetadataContext? metadata)
        {
            var components = GetComponents<IValidatorListener>(metadata);
            for (var i = 0; i < components.Length; i++)
                components[i].OnErrorsChanged(this, memberName, metadata);
        }

        protected virtual void OnAsyncValidation(IValidator validator, string memberName, Task validationTask, IReadOnlyMetadataContext? metadata)
        {
            var components = GetComponents<IValidatorListener>(metadata);
            for (var i = 0; i < components.Length; i++)
                components[i].OnAsyncValidation(this, memberName, validationTask, metadata);
        }

        protected virtual void OnDispose(IValidator validator)
        {
            Components.Remove(validator);
        }

        protected virtual void OnValidatorAdded(IValidator validator, IReadOnlyMetadataContext? metadata)
        {
            validator.AddComponent(this);
        }

        protected virtual bool OnValidatorAdding(IValidator validator, IReadOnlyMetadataContext? metadata)
        {
            return Array.IndexOf(Components.GetComponents<IValidator>(metadata), validator) < 0;
        }

        protected virtual void OnValidatorRemoved(IValidator validator, IReadOnlyMetadataContext? metadata)
        {
            validator.RemoveComponent(this);
        }

        private void EnsureInitialized()
        {
            if (_state != InitializedState)
                ExceptionManager.ThrowObjectNotInitialized(this);
        }

        #endregion
    }
}