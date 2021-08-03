using System.Runtime.CompilerServices;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Components
{
    public abstract class MultiAttachableComponentBase<T> : IAttachableComponent, IDetachableComponent where T : class
    {
        private object? _owners;

        protected ItemOrArray<T> Owners
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ItemOrArray.FromRawValue<T>(_owners);
        }

        public virtual void OnAttaching(object owner, IReadOnlyMetadataContext? metadata)
        {
            if (owner is not T o)
                return;

            lock (this)
            {
                MugenExtensions.AddRaw(ref _owners, o);
            }

            OnAttaching(o, metadata);
        }

        public virtual void OnAttached(object owner, IReadOnlyMetadataContext? metadata)
        {
            if (owner is T o)
                OnAttached(o, metadata);
        }

        public virtual void OnDetaching(object owner, IReadOnlyMetadataContext? metadata)
        {
            if (owner is T o)
                OnDetaching(o, metadata);
        }

        public virtual void OnDetached(object owner, IReadOnlyMetadataContext? metadata)
        {
            if (owner is not T o)
                return;

            OnDetached(o, metadata);
            lock (this)
            {
                MugenExtensions.RemoveRaw(ref _owners, o);
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
    }
}