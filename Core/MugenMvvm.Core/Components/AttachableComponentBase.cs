using System.Diagnostics.CodeAnalysis;
using System.Threading;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Components
{
    public abstract class AttachableComponentBase<T> : IAttachableComponent, IDetachableComponent where T : class
    {
        #region Fields

        private T? _owner;
        private int _state;
        private const int DetachedState = 0;
        private const int AttachedState = 1;

        #endregion

        #region Properties

        [AllowNull]
        protected T Owner
        {
            get
            {
                if (_owner == null)
                    ExceptionManager.ThrowObjectNotInitialized(this, nameof(Owner));
                return _owner;
            }
            private set => _owner = value;
        }

        protected bool IsAttached => _state == AttachedState;

        #endregion

        #region Implementation of interfaces

        bool IAttachableComponent.OnAttaching(object owner, IReadOnlyMetadataContext? metadata)
        {
            if (owner is T o)
                return OnAttachingInternal(o, metadata);
            return true;
        }

        void IAttachableComponent.OnAttached(object owner, IReadOnlyMetadataContext? metadata)
        {
            if (!(owner is T o))
                return;

            if (Interlocked.CompareExchange(ref _state, AttachedState, DetachedState) != DetachedState)
                ExceptionManager.ThrowObjectInitialized(this);

            Owner = o;
            OnAttachedInternal(o, metadata);
        }

        bool IDetachableComponent.OnDetaching(object owner, IReadOnlyMetadataContext? metadata)
        {
            if (owner is T o)
                return OnDetachingInternal(o, metadata);
            return true;
        }

        void IDetachableComponent.OnDetached(object owner, IReadOnlyMetadataContext? metadata)
        {
            if (owner is T o && ReferenceEquals(Owner, o) && Interlocked.Exchange(ref _state, DetachedState) != DetachedState)
            {
                OnDetachedInternal(o, metadata);
                Owner = null;
            }
        }

        #endregion

        #region Methods

        protected virtual bool OnAttachingInternal(T owner, IReadOnlyMetadataContext? metadata)
        {
            return true;
        }

        protected virtual void OnAttachedInternal(T owner, IReadOnlyMetadataContext? metadata)
        {
        }

        protected virtual bool OnDetachingInternal(T owner, IReadOnlyMetadataContext? metadata)
        {
            return true;
        }

        protected virtual void OnDetachedInternal(T owner, IReadOnlyMetadataContext? metadata)
        {
        }

        #endregion
    }
}