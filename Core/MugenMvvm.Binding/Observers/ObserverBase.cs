using System;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Observers
{
    public abstract class ObserverBase : IMemberPathObserver
    {
        #region Fields

        private object? _listeners;
        private object? _target;

        protected const byte ObservableFlag = 1 << 1;
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
                if (_target is IWeakReference w)
                    return w.Target != null;
                return true;
            }
        }

        public object? Target
        {
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
            if (_listeners == null)
                _listeners = listener;
            else if (_listeners is IMemberPathObserverListener[] listeners)
            {
                Array.Resize(ref listeners, listeners.Length + 1);
                listeners[listeners.Length - 1] = listener;
                _listeners = listeners;
            }
            else
                _listeners = new[] { (IMemberPathObserverListener)_listeners, listener };

            OnListenerAdded(listener);
        }

        public void RemoveListener(IMemberPathObserverListener listener)
        {
            Should.NotBeNull(listener, nameof(listener));
            if (!IsDisposed && RemoveListenerInternal(listener) && _listeners == null)
                OnListenersRemoved();
        }

        public abstract MemberPathMembers GetMembers(IReadOnlyMetadataContext? metadata = null);

        public abstract MemberPathLastMember GetLastMember(IReadOnlyMetadataContext? metadata = null);

        #endregion

        #region Methods

        protected static Type GetTargetType(object target, MemberFlags flags)
        {
            if (flags.HasFlagEx(MemberFlags.Static))
                return target as Type ?? target.GetType();
            return target.GetType();
        }

        protected virtual void OnListenerAdded(IMemberPathObserverListener listener)
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

        #endregion
    }
}