using System.Diagnostics.CodeAnalysis;
using System.Threading;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Components
{
    public abstract class AttachableComponentBase<T> : IAttachableComponent, IDetachableComponent where T : class
    {
        #region Fields

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
                if (OwnerOptional == null)
                    ExceptionManager.ThrowObjectNotInitialized(this, nameof(Owner));
                return OwnerOptional;
            }
            private set => OwnerOptional = value;
        }

        protected T? OwnerOptional { get; private set; }

        protected bool IsAttached => _state == AttachedState;

        #endregion

        #region Implementation of interfaces

        bool IAttachableComponent.OnAttaching(object owner, IReadOnlyMetadataContext? metadata)
        {
            if (owner is T o)
                return OnAttaching(o, metadata);
            return true;
        }

        void IAttachableComponent.OnAttached(object owner, IReadOnlyMetadataContext? metadata)
        {
            if (!(owner is T o))
                return;

            if (Interlocked.CompareExchange(ref _state, AttachedState, DetachedState) != DetachedState)
                ExceptionManager.ThrowObjectInitialized(this);

            Owner = o;
            OnAttached(o, metadata);
        }

        bool IDetachableComponent.OnDetaching(object owner, IReadOnlyMetadataContext? metadata)
        {
            if (owner is T o)
                return OnDetaching(o, metadata);
            return true;
        }

        void IDetachableComponent.OnDetached(object owner, IReadOnlyMetadataContext? metadata)
        {
            if (owner is T o && ReferenceEquals(OwnerOptional, o) && Interlocked.Exchange(ref _state, DetachedState) != DetachedState)
            {
                OnDetached(o, metadata);
                Owner = null;
            }
        }

        #endregion

        #region Methods

        protected virtual bool OnAttaching(T owner, IReadOnlyMetadataContext? metadata)
        {
            return true;
        }

        protected virtual void OnAttached(T owner, IReadOnlyMetadataContext? metadata)
        {
        }

        protected virtual bool OnDetaching(T owner, IReadOnlyMetadataContext? metadata)
        {
            return true;
        }

        protected virtual void OnDetached(T owner, IReadOnlyMetadataContext? metadata)
        {
        }

        #endregion
    }
}