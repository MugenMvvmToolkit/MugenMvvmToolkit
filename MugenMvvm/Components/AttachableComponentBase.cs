using System.Runtime.CompilerServices;
using System.Threading;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Components
{
    public abstract class AttachableComponentBase<T> : IAttachableComponent, IDetachableComponent where T : class
    {
        private T? _owner;

        protected internal T Owner
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (_owner == null)
                    ExceptionManager.ThrowObjectNotInitialized(this, nameof(Owner));
                return _owner;
            }
        }

        protected internal T? OwnerOptional
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _owner;
        }

        protected bool IsAttached => _owner != null;

        public virtual bool OnAttaching(object owner, IReadOnlyMetadataContext? metadata)
        {
            if (owner is T o)
                return OnAttaching(o, metadata);
            return true;
        }

        public virtual void OnAttached(object owner, IReadOnlyMetadataContext? metadata)
        {
            if (owner is not T o)
                return;

            if (Interlocked.CompareExchange(ref _owner, o, null) != null)
                ExceptionManager.ThrowObjectInitialized(this);
            OnAttached(o, metadata);
        }

        public virtual bool OnDetaching(object owner, IReadOnlyMetadataContext? metadata)
        {
            if (owner is T o)
                return OnDetaching(o, metadata);
            return true;
        }

        public virtual void OnDetached(object owner, IReadOnlyMetadataContext? metadata)
        {
            if (owner is T o)
            {
                OnDetached(o, metadata);
                Interlocked.CompareExchange(ref _owner, null, o);
            }
        }

        protected virtual bool OnAttaching(T owner, IReadOnlyMetadataContext? metadata) => OwnerOptional == null;

        protected virtual void OnAttached(T owner, IReadOnlyMetadataContext? metadata)
        {
        }

        protected virtual bool OnDetaching(T owner, IReadOnlyMetadataContext? metadata) => true;

        protected virtual void OnDetached(T owner, IReadOnlyMetadataContext? metadata)
        {
        }
    }
}