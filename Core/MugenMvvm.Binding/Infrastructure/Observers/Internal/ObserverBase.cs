using System;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;

// ReSharper disable once CheckNamespace
namespace MugenMvvm.Binding.Infrastructure.Observers
{
    internal abstract class ObserverBase : IBindingPathObserver
    {
        #region Fields

        private IBindingPathObserverListener[] _listeners;
        private readonly IBindingMemberInfo? _member;
        private IWeakReference? _source;
        private IDisposable? _unsubscriber;

        private static readonly IBindingPathObserverListener[] DisposedItems = new IBindingPathObserverListener[0];

        #endregion

        #region Constructors

        protected ObserverBase(IWeakReference source, IBindingMemberInfo? member)
        {
            _source = source;
            _member = member;
            _listeners = Default.EmptyArray<IBindingPathObserverListener>();
        }

        #endregion

        #region Properties

        public bool IsAlive=> _source?.Target != null;

        public abstract IBindingPath Path { get; }

        public object Source => _source?.Target;

        protected bool HasListeners => _listeners.Length > 0;

        protected bool HasSourceListener => _unsubscriber != null;

        protected bool IsDisposed => ReferenceEquals(_listeners, DisposedItems);

        #endregion

        #region Implementation of interfaces

        public void Dispose()
        {
            if (ReferenceEquals(_listeners, DisposedItems))
                return;
            _listeners = DisposedItems;
            _unsubscriber?.Dispose();
            _source?.Release();
            _source = null;
            _unsubscriber = null;
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
                _listeners = Default.EmptyArray<IBindingPathObserverListener>();
            else
                Array.Resize(ref _listeners, _listeners.Length - 1);
        }

        public abstract BindingPathMembers GetMembers(IReadOnlyMetadataContext metadata);

        public abstract BindingPathLastMember GetLastMember(IReadOnlyMetadataContext metadata);

        #endregion

        #region Methods

        protected abstract IBindingEventListener GetSourceListener();

        protected virtual void OnListenerAdded(IBindingPathObserverListener listener)
        {
        }

        protected virtual void OnDisposed()
        {
        }

        protected void AddSourceListener()
        {
            if (_unsubscriber != null)
                return;
            if (_member != null && _member.CanObserve)
            {
                var source = Source;
                if (source != null)
                    _unsubscriber = _member.TryObserve(source, GetSourceListener(), null);
            }

            if (_unsubscriber == null)
                _unsubscriber = Default.Disposable;
        }

        protected IWeakReference GetSourceWeakReference(object source)
        {
            if (_member == null)
                return _source;
            return Service<IWeakReferenceProvider>.Instance.GetWeakReference(source, Default.Metadata);
        }

        protected bool TryGetSourceValue(out object? value)
        {
            var source = Source;
            if (source == null)
            {
                value = null;
                return false;
            }

            value = _member.GetValue(source, null, null);
            return true;
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