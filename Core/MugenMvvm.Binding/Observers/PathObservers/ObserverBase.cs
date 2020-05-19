using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Observers.PathObservers
{
    public abstract class ObserverBase : IMemberPathObserver
    {
        #region Fields

        private object? _listeners;
        private object? _target;

        protected const byte UpdatingFlag = 1 << 1;
        protected const byte OptionalFlag = 1 << 2;
        protected const byte HasStablePathFlag = 1 << 3;
        protected const byte InitializedFlag = 1 << 4;

        private static readonly IMemberPathObserverListener[] DisposedItems = new IMemberPathObserverListener[0];

        #endregion

        #region Constructors

        protected ObserverBase(object target)
        {
            Should.NotBeNull(target, nameof(target));
            _target = target;
        }

        #endregion

        #region Properties

        public bool IsAlive
        {
            get
            {
                if (_target is IWeakItem w)
                    return w.IsAlive;
                return !IsDisposed;
            }
        }

        public object? Target
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (_target is IWeakReference w)
                    return w.Target;
                return _target;
            }
        }

        public abstract IMemberPath Path { get; }

        protected bool HasListeners => _listeners != null;

        protected bool IsDisposed => ReferenceEquals(_listeners, DisposedItems);

        #endregion

        #region Implementation of interfaces

        public void Dispose()
        {
            if (ReferenceEquals(_listeners, DisposedItems))
                return;
            _listeners = DisposedItems;
            _target = null;
            OnDisposed();
        }

        public void AddListener(IMemberPathObserverListener listener)
        {
            Should.NotBeNull(listener, nameof(listener));
            if (IsDisposed)
                return;
            var oldListeners = _listeners;
            if (oldListeners == null)
                _listeners = listener;
            else if (oldListeners is IMemberPathObserverListener[] listeners)
            {
                Array.Resize(ref listeners, listeners.Length + 1);
                listeners[listeners.Length - 1] = listener;
                _listeners = listeners;
            }
            else
                _listeners = new[] { (IMemberPathObserverListener)oldListeners, listener };

            if (oldListeners == null)
                OnListenersAdded();
        }

        public void RemoveListener(IMemberPathObserverListener listener)
        {
            Should.NotBeNull(listener, nameof(listener));
            if (!IsDisposed && RemoveListenerInternal(listener) && _listeners == null)
                OnListenersRemoved();
        }

        public ItemOrList<IMemberPathObserverListener, IReadOnlyList<IMemberPathObserverListener>> GetListeners()
        {
            return ItemOrList<IMemberPathObserverListener, IReadOnlyList<IMemberPathObserverListener>>.FromRawValue(_listeners);
        }

        public abstract MemberPathMembers GetMembers(IReadOnlyMetadataContext? metadata = null);

        public abstract MemberPathLastMember GetLastMember(IReadOnlyMetadataContext? metadata = null);

        #endregion

        #region Methods

        protected IReadOnlyMetadataContext? TryGetMetadata()
        {
            if (_listeners is IMemberPathObserverListener[] l)
            {
                for (var i = 0; i < l.Length; i++)
                {
                    var metadata = TryGetMetadata(l[i]);
                    if (metadata != null)
                        return metadata;
                }

                return null;
            }

            return TryGetMetadata(_listeners);
        }

        protected virtual void OnListenersAdded()
        {
        }

        protected virtual void OnListenersRemoved()
        {
        }

        protected virtual void OnDisposed()
        {
        }

        protected void OnPathMembersChanged()
        {
            try
            {
                var listeners = _listeners;
                if (listeners is IMemberPathObserverListener[] l)
                {
                    for (var i = 0; i < l.Length; i++)
                        l[i].OnPathMembersChanged(this);
                }
                else
                    (listeners as IMemberPathObserverListener)?.OnPathMembersChanged(this);
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }

        protected virtual void OnLastMemberChanged()
        {
            try
            {
                var listeners = _listeners;
                if (listeners is IMemberPathObserverListener[] l)
                {
                    for (var i = 0; i < l.Length; i++)
                        l[i].OnLastMemberChanged(this);
                }
                else
                    (listeners as IMemberPathObserverListener)?.OnLastMemberChanged(this);
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }

        protected void OnError(Exception exception)
        {
            try
            {
                var listeners = _listeners;
                if (listeners is IMemberPathObserverListener[] l)
                {
                    for (var i = 0; i < l.Length; i++)
                        l[i].OnError(this, exception);
                }
                else
                    (listeners as IMemberPathObserverListener)?.OnError(this, exception);
            }
            catch
            {
                ;
            }
        }

        private bool RemoveListenerInternal(IMemberPathObserverListener listener)
        {
            if (_listeners == null)
                return false;

            if (ReferenceEquals(listener, _listeners))
            {
                _listeners = null;
                return true;
            }

            if (!(_listeners is IMemberPathObserverListener[] items))
                return false;

            if (items.Length == 2)
            {
                if (ReferenceEquals(items[0], listener))
                {
                    _listeners = items[1];
                    return true;
                }

                if (ReferenceEquals(items[1], listener))
                {
                    _listeners = items[0];
                    return true;
                }
            }
            else if (MugenExtensions.Remove(ref items, listener))
            {
                _listeners = items;
                return true;
            }

            return false;
        }

        private static IReadOnlyMetadataContext? TryGetMetadata(object? value)
        {
            if (value is IMetadataOwner<IReadOnlyMetadataContext> metadataOwner && metadataOwner.HasMetadata)
                return metadataOwner.GetMetadataOrDefault();
            return null;
        }

        #endregion

        #region Nested types

        protected internal interface IMethodPathObserver : IMemberPathObserver
        {
            MemberFlags MemberFlags { get; }

            string Method { get; }

            IEventListener GetMethodListener();
        }

        #endregion
    }
}