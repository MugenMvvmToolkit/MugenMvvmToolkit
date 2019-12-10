using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Attributes;
using MugenMvvm.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;

namespace MugenMvvm.Validation
{
    public sealed class AggregatorValidator : ComponentOwnerBase<IValidator>, IAggregatorValidator, IValidatorListener, IHasAddedCallbackComponentOwner, IHasAddingCallbackComponentOwner, IHasRemovedCallbackComponentOwner
    {
        #region Fields

        private readonly IMetadataContextProvider? _metadataContextProvider;

        private InlineValidator<object>? _inlineValidator;
        private IMetadataContext? _metadata;
        private int _state;

        private const int DisposedState = -1;

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

        public bool HasErrors => !IsDisposed && GetComponents<IValidator>().HasErrors();

        public bool IsDisposed => _state == DisposedState;

        #endregion

        #region Implementation of interfaces

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _state, DisposedState) == DisposedState)
                return;
            GetComponents<IValidatorListener>().OnDisposed(this);
            this.ClearComponents();
            this.ClearMetadata(true);
        }

        public IReadOnlyList<object> GetErrors(string? memberName, IReadOnlyMetadataContext? metadata = null)
        {
            return GetComponents<IValidator>(metadata).TryGetErrors(memberName, metadata) ?? Default.EmptyArray<object>();
        }

        public IReadOnlyDictionary<string, IReadOnlyList<object>> GetErrors(IReadOnlyMetadataContext? metadata = null)
        {
            return GetComponents<IValidator>(metadata).TryGetErrors(metadata) ?? Default.ReadOnlyDictionary<string, IReadOnlyList<object>>();
        }

        public Task ValidateAsync(string? memberName = null, CancellationToken cancellationToken = default, IReadOnlyMetadataContext? metadata = null)
        {
            if (IsDisposed)
                return Default.CompletedTask;
            return GetComponents<IValidator>(metadata).ValidateAsync(memberName, cancellationToken, metadata);
        }

        public void ClearErrors(string? memberName = null, IReadOnlyMetadataContext? metadata = null)
        {
            if (IsDisposed)
                return;
            GetComponents<IValidator>(metadata).ClearErrors(memberName, metadata);
        }

        public void SetErrors(string memberName, IReadOnlyList<object> errors, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(memberName, nameof(memberName));
            Should.NotBeNull(errors, nameof(errors));
            if (IsDisposed)
                ExceptionManager.ThrowObjectDisposed(GetType());
            SetErrorsInternal(memberName, errors, metadata);
        }

        void IHasAddedCallbackComponentOwner.OnComponentAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (component is IValidator validator)
                validator.AddComponent(this, metadata);
        }

        bool IHasAddingCallbackComponentOwner.OnComponentAdding(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (ReferenceEquals(this, component))
                return false;
            if (component is IValidator validator)
                return Array.IndexOf(Components.Get<IValidator>(metadata), validator) < 0;
            return true;
        }

        void IHasRemovedCallbackComponentOwner.OnComponentRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (component is IValidator validator)
                validator.RemoveComponent(this, metadata);
        }

        void IValidatorListener.OnErrorsChanged(IValidator validator, string memberName, IReadOnlyMetadataContext? metadata)
        {
            GetComponents<IValidatorListener>(metadata).OnErrorsChanged(this, memberName, metadata);
        }

        void IValidatorListener.OnAsyncValidation(IValidator validator, string memberName, Task validationTask, IReadOnlyMetadataContext? metadata)
        {
            GetComponents<IValidatorListener>(metadata).OnAsyncValidation(this, memberName, validationTask, metadata);
        }

        void IValidatorListener.OnDisposed(IValidator validator)
        {
            Components.Remove(validator);
        }

        #endregion

        #region Methods

        private void SetErrorsInternal(string memberName, IReadOnlyList<object> errors, IReadOnlyMetadataContext? metadata = null)
        {
            if (_inlineValidator == null && MugenExtensions.LazyInitialize(ref _inlineValidator, new InlineValidator<object>()))
            {
                _inlineValidator.Initialize(this);
                _inlineValidator.AddComponent(this, metadata);
                Components.Add(_inlineValidator, metadata);
            }

            _inlineValidator.SetErrors(memberName, errors, metadata);
        }

        #endregion
    }
}