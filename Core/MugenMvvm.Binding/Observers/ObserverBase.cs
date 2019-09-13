using System;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Observers
{
    public abstract class ObserverBase : IBindingPathObserver
    {
        #region Fields

        protected const byte IgnoreAttachedMembersFlag = 1;
        protected const byte ObservableFlag = 1 << 1;
        protected const byte OptionalFlag = 1 << 2;
        protected const byte HasStablePathFlag = 1 << 3;
        protected const byte InitializedFlag = 1 << 4;

        private IBindingPathObserverListener[] _listeners;//todo change to item or array
        private IWeakReference? _source;

        private static readonly IBindingPathObserverListener[] DisposedItems = new IBindingPathObserverListener[0];

        #endregion

        #region Constructors

        protected ObserverBase(IWeakReference source)
        {
            _source = source;
            _listeners = Default.EmptyArray<IBindingPathObserverListener>();
        }

        #endregion

        #region Properties

        public bool IsAlive => _source?.Target != null;

        public abstract IBindingPath Path { get; }

        public object? Source => _source?.Target;

        protected IWeakReference SourceWeakReference => _source!;

        protected bool HasListeners => _listeners.Length > 0;

        protected bool IsDisposed => ReferenceEquals(_listeners, DisposedItems);

        #endregion

        #region Implementation of interfaces

        public void Dispose()
        {
            if (ReferenceEquals(_listeners, DisposedItems))
                return;
            _listeners = DisposedItems;
            _source?.Release();
            _source = null;
            OnDisposed();
        }

        public void AddListener(IBindingPathObserverListener listener)
        {
            Should.NotBeNull(listener, nameof(listener));
            if (IsDisposed)
                return;
            Array.Resize(ref _listeners, _listeners.Length + 1);
            _listeners[_listeners.Length - 1] = listener;
            OnListenerAdded(listener);
        }

        public void RemoveListener(IBindingPathObserverListener listener)
        {
            Should.NotBeNull(listener, nameof(listener));
            if (IsDisposed)
                return;
            var resize = false;
            for (var i = 0; i < _listeners.Length - 1; i++)
            {
                if (resize)
                    _listeners[i] = _listeners[i + 1];
                else if (ReferenceEquals(_listeners[i], listener))
                {
                    _listeners[i] = _listeners[i + 1];
                    resize = true;
                }
            }

            if (!resize && !ReferenceEquals(_listeners[_listeners.Length], listener))
                return;

            if (_listeners.Length == 1)
            {
                _listeners = Default.EmptyArray<IBindingPathObserverListener>();
                OnListenersRemoved();
            }
            else
                Array.Resize(ref _listeners, _listeners.Length - 1);
        }

        public abstract BindingPathMembers GetMembers(IReadOnlyMetadataContext? metadata = null);

        public abstract BindingPathLastMember GetLastMember(IReadOnlyMetadataContext? metadata = null);

        #endregion

        #region Methods

        protected virtual void OnListenerAdded(IBindingPathObserverListener listener)
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
                for (var i = 0; i < _listeners.Length; i++)
                    _listeners[i].OnPathMembersChanged(this);
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }

        protected void OnLastMemberChanged()
        {
            try
            {
                for (var i = 0; i < _listeners.Length; i++)
                    _listeners[i].OnLastMemberChanged(this);
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
                for (var i = 0; i < _listeners.Length; i++)
                    _listeners[i].OnError(this, exception);
            }
            catch
            {
                ;
            }
        }

        protected static IBindingMemberInfo? GetBindingMember(Type type, string path, bool ignoreAttached)
        {
            if (ignoreAttached)
                return Service<IBindingMemberProvider>.Instance.GetRawMember(type, path, Default.Metadata);
            return Service<IBindingMemberProvider>.Instance.GetMember(type, path, Default.Metadata);
        }

        #endregion
    }
}