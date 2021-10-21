using System.Diagnostics.CodeAnalysis;
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

        [MemberNotNullWhen(true, nameof(OwnerOptional))]
        protected bool IsAttached => _owner != null;

        protected virtual void OnAttaching(object owner, IReadOnlyMetadataContext? metadata)
        {
            if (owner is not T o)
                return;

            if (Interlocked.CompareExchange(ref _owner, o, null) != null)
                ExceptionManager.ThrowObjectInitialized(this);
            OnAttaching(o, metadata);
        }

        protected virtual void OnAttached(object owner, IReadOnlyMetadataContext? metadata)
        {
            if (owner is T o)
                OnAttached(o, metadata);
        }

        protected virtual void OnDetaching(object owner, IReadOnlyMetadataContext? metadata)
        {
            if (owner is T o)
                OnDetaching(o, metadata);
        }

        protected virtual void OnDetached(object owner, IReadOnlyMetadataContext? metadata)
        {
            if (owner is T o)
            {
                OnDetached(o, metadata);
                Interlocked.CompareExchange(ref _owner, null, o);
            }
        }

        protected virtual void OnAttaching(T owner, IReadOnlyMetadataContext? metadata)
        {
        }

        protected virtual void OnAttached(T owner, IReadOnlyMetadataContext? metadata)
        {
        }

        protected virtual void OnDetaching(T owner, IReadOnlyMetadataContext? metadata)
        {
        }

        protected virtual void OnDetached(T owner, IReadOnlyMetadataContext? metadata)
        {
        }

        void IAttachableComponent.OnAttaching(object owner, IReadOnlyMetadataContext? metadata) => OnAttaching(owner, metadata);

        void IAttachableComponent.OnAttached(object owner, IReadOnlyMetadataContext? metadata) => OnAttached(owner, metadata);

        void IDetachableComponent.OnDetaching(object owner, IReadOnlyMetadataContext? metadata) => OnDetaching(owner, metadata);

        void IDetachableComponent.OnDetached(object owner, IReadOnlyMetadataContext? metadata) => OnDetached(owner, metadata);
    }
}