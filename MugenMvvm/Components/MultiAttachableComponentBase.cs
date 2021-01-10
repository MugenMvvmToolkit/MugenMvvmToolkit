using System;
using System.Runtime.CompilerServices;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Components
{
    public abstract class MultiAttachableComponentBase<T> : IAttachableComponent, IDetachableComponent where T : class
    {
        #region Fields

        private object? _owners;

        #endregion

        #region Properties

        protected ItemOrArray<T> Owners
        {
            // ReSharper disable once InconsistentlySynchronizedField
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ItemOrArray.FromRawValue<T>(_owners);
        }

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

            lock (this)
            {
                if (_owners == null)
                    _owners = o;
                else if (_owners is T[] items)
                {
                    Array.Resize(ref items, items.Length + 1);
                    items[items.Length - 1] = o;
                    _owners = items;
                }
                else
                    _owners = new[] {(T) _owners, o};
            }

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
            if (!(owner is T o))
                return;

            OnDetached(o, metadata);
            lock (this)
            {
                MugenExtensions.RemoveRaw(ref _owners, o);
            }
        }

        #endregion

        #region Methods

        protected virtual bool OnAttaching(T owner, IReadOnlyMetadataContext? metadata) => true;

        protected virtual void OnAttached(T owner, IReadOnlyMetadataContext? metadata)
        {
        }

        protected virtual bool OnDetaching(T owner, IReadOnlyMetadataContext? metadata) => true;

        protected virtual void OnDetached(T owner, IReadOnlyMetadataContext? metadata)
        {
        }

        #endregion
    }
}