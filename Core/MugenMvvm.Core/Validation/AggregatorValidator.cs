using System.Collections.Generic;
using System.Linq;
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
    public class AggregatorValidator : ComponentOwnerBase<IValidator>, IAggregatorValidator, IValidatorListener, IComponentOwnerAddedCallback<IValidator>,
        IComponentOwnerAddingCallback<IValidator>, IComponentOwnerRemovedCallback<IValidator>
    {
        #region Fields

        private InlineValidator<object>? _inlineValidator;
        private IMetadataContext? _metadata;
        private int _state;
        private IComponentCollection<IValidator>? _validators;
        private readonly IMetadataContextProvider? _metadataContextProvider;

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

        public IComponentCollection<IValidator> Validators
        {
            get
            {
                if (_validators == null)
                    ComponentCollectionProvider.LazyInitialize(ref _validators, this);
                return _validators;
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

        void IComponentOwnerAddedCallback<IValidator>.OnComponentAdded(IComponentCollection<IValidator> collection, IValidator component, IReadOnlyMetadataContext? metadata)
        {
            OnValidatorAdded(component, metadata);
        }

        bool IComponentOwnerAddingCallback<IValidator>.OnComponentAdding(IComponentCollection<IValidator> collection, IValidator component, IReadOnlyMetadataContext? metadata)
        {
            return OnValidatorAdding(component, metadata);
        }

        void IComponentOwnerRemovedCallback<IValidator>.OnComponentRemoved(IComponentCollection<IValidator> collection, IValidator component, IReadOnlyMetadataContext? metadata)
        {
            OnValidatorRemoved(component, metadata);
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
            var validators = GetValidators();
            for (var i = 0; i < validators.Length; i++)
            {
                var list = validators[i].GetErrors(memberName, metadata);
                if (list.Count == 0)
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
            var validators = GetValidators();
            for (var i = 0; i < validators.Length; i++)
            {
                var dictionary = validators[i].GetErrors(metadata);
                if (dictionary.Count == 0)
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
            var validators = GetValidators();
            if (validators.Length == 0)
                return Default.CompletedTask;

            var tasks = new Task[validators.Length];
            for (var i = 0; i < validators.Length; i++)
                tasks[i] = validators[i].ValidateAsync(memberName, cancellationToken, metadata);
            return Task.WhenAll(tasks);
        }

        protected virtual void ClearErrorsInternal(string? memberName, IReadOnlyMetadataContext? metadata)
        {
            var validators = GetValidators();
            for (var i = 0; i < validators.Length; i++)
                validators[i].ClearErrors(memberName, metadata);
        }

        protected void SetErrorsInternal(string memberName, IReadOnlyList<object> errors, IReadOnlyMetadataContext? metadata = null)
        {
            if (_inlineValidator == null && MugenExtensions.LazyInitialize(ref _inlineValidator, new InlineValidator<object>()))
            {
                _inlineValidator.Initialize(this, null);
                _inlineValidator.AddComponent(this);
                Validators.Add(_inlineValidator);
            }

            _inlineValidator.SetErrors(memberName, errors, metadata);
        }

        protected virtual bool HasErrorsInternal()
        {
            var validators = GetValidators();
            for (var i = 0; i < validators.Length; i++)
            {
                if (validators[i].HasErrors)
                    return true;
            }

            return false;
        }

        protected virtual void OnInitialized(IReadOnlyMetadataContext? metadata)
        {
        }

        protected virtual void OnDispose()
        {
            var components = GetComponents();
            for (var i = 0; i < components.Length; i++)
                (components[i] as IValidatorListener)?.OnDisposed(this);
        }

        protected virtual void OnErrorsChanged(IValidator validator, string memberName, IReadOnlyMetadataContext? metadata)
        {
            var components = GetComponents();
            for (var i = 0; i < components.Length; i++)
                (components[i] as IValidatorListener)?.OnErrorsChanged(this, memberName, metadata);
        }

        protected virtual void OnAsyncValidation(IValidator validator, string memberName, Task validationTask, IReadOnlyMetadataContext? metadata)
        {
            var components = GetComponents();
            for (var i = 0; i < components.Length; i++)
                (components[i] as IValidatorListener)?.OnAsyncValidation(this, memberName, validationTask, metadata);
        }

        protected virtual void OnDispose(IValidator validator)
        {
            Validators.Remove(validator);
        }

        protected virtual void OnValidatorAdded(IValidator validator, IReadOnlyMetadataContext? metadata)
        {
            validator.AddComponent(this);
        }

        protected virtual bool OnValidatorAdding(IValidator validator, IReadOnlyMetadataContext? metadata)
        {
            return !Validators.GetComponents().Contains(validator);
        }

        protected virtual void OnValidatorRemoved(IValidator validator, IReadOnlyMetadataContext? metadata)
        {
            validator.RemoveComponent(this);
        }

        protected IValidator[] GetValidators()
        {
            return _validators.GetItemsOrDefault();
        }

        private void EnsureInitialized()
        {
            if (_state != InitializedState)
                ExceptionManager.ThrowObjectNotInitialized(this);
        }

        #endregion
    }
}