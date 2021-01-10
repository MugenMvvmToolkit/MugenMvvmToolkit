using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Attributes;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Validation
{
    public sealed class Validator : ComponentOwnerBase<IValidator>, IValidator, IHasComponentAddedHandler, IHasComponentRemovedHandler, IHasComponentAddingHandler, IHasDisposeCondition
    {
        #region Fields

        private IReadOnlyMetadataContext? _metadata;
        private int _state;

        private const int DefaultState = 0;
        private const int NoDisposeState = 1;
        private const int DisposedState = 2;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public Validator(IReadOnlyMetadataContext? metadata = null, IComponentCollectionManager? componentCollectionManager = null)
            : base(componentCollectionManager)
        {
            _metadata = metadata;
        }

        #endregion

        #region Properties

        public bool HasMetadata => !_metadata.IsNullOrEmpty();

        public IMetadataContext Metadata => _metadata as IMetadataContext ?? MugenExtensions.EnsureInitialized(ref _metadata);

        public bool IsDisposed => _state == DisposedState;

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

        void IHasComponentAddedHandler.OnComponentAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (component is IValidatorComponent)
                GetComponents<IValidatorListener>().OnErrorsChanged(this, null, string.Empty, metadata);
        }

        bool IHasComponentAddingHandler.OnComponentAdding(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) => !IsDisposed;

        void IHasComponentRemovedHandler.OnComponentRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (component is IValidatorComponent)
                GetComponents<IValidatorListener>().OnErrorsChanged(this, null, string.Empty, metadata);
        }

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _state, DisposedState, DefaultState) == DefaultState)
            {
                base.GetComponents<IValidatorListener>().OnDisposed(this);
                base.GetComponents<IDisposable>().Dispose();
                this.ClearComponents();
                this.ClearMetadata(true);
            }
        }

        public bool HasErrors(string? memberName = null, IReadOnlyMetadataContext? metadata = null) => GetComponents<IValidatorComponent>(metadata).HasErrors(this, memberName, metadata);

        public ItemOrIReadOnlyList<object> GetErrors(string? memberName, IReadOnlyMetadataContext? metadata = null) => GetComponents<IValidatorComponent>(metadata).TryGetErrors(this, memberName, metadata);

        public IReadOnlyDictionary<string, object> GetErrors(IReadOnlyMetadataContext? metadata = null) =>
            GetComponents<IValidatorComponent>(metadata).TryGetErrors(this, metadata) ?? Default.ReadOnlyDictionary<string, object>();

        public Task ValidateAsync(string? memberName = null, CancellationToken cancellationToken = default, IReadOnlyMetadataContext? metadata = null) =>
            GetComponents<IValidatorComponent>(metadata).TryValidateAsync(this, memberName, cancellationToken, metadata);

        public void ClearErrors(string? memberName = null, IReadOnlyMetadataContext? metadata = null) => GetComponents<IValidatorComponent>(metadata).ClearErrors(this, memberName, metadata);

        #endregion

        #region Methods

        private new TComponent[] GetComponents<TComponent>(IReadOnlyMetadataContext? metadata = null)
            where TComponent : class =>
            IsDisposed ? Default.Array<TComponent>() : base.GetComponents<TComponent>(metadata);

        #endregion
    }
}