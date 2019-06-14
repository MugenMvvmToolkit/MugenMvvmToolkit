using System;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;

// ReSharper disable once CheckNamespace
namespace MugenMvvm.Binding.Infrastructure.Observers
{
    internal sealed class MultiPathObserver : ObserverBase, IBindingEventListener
    {
        #region Fields

        private readonly bool _hasStablePath;
        private readonly bool _ignoreAttachedMembers;

        private readonly IDisposable[]? _listeners;
        private readonly bool _observable;
        private readonly bool _optional;
        private Exception? _exception;
        private IBindingEventListener? _lastMemberListener;
        private IBindingMemberInfo[]? _members;
        private IWeakReference? _penultimateValue;

        #endregion

        #region Constructors

        public MultiPathObserver(IWeakReference source, IBindingMemberInfo member, IBindingPath path, bool ignoreAttachedMembers, bool hasStablePath, bool observable, bool optional)
            : base(source, member)
        {
            _ignoreAttachedMembers = ignoreAttachedMembers;
            _hasStablePath = hasStablePath;
            _observable = observable;
            _optional = optional;
            if (observable)
                _listeners = new IDisposable[path.Parts.Length];
            Path = path;
        }

        #endregion

        #region Properties

        public override IBindingPath Path { get; }

        public bool IsWeak => true;

        #endregion

        #region Implementation of interfaces

        bool IBindingEventListener.TryHandle(object sender, object message)
        {
            return Update();
        }

        #endregion

        #region Methods

        public override BindingPathMembers GetMembers(IReadOnlyMetadataContext metadata)
        {
            UpdateIfNeed();
            if (_exception != null)
                return new BindingPathMembers(Path, _exception);

            var target = _penultimateValue?.Target;
            if (target == null)
                return default;

            if (!TryGetSourceValue(out var source))
                return default;

            return new BindingPathMembers(Path, source, target, _members, _members[_members.Length - 1]);
        }

        public override BindingPathLastMember GetLastMember(IReadOnlyMetadataContext metadata)
        {
            UpdateIfNeed();
            if (_exception != null)
                return new BindingPathLastMember(Path, _exception);

            var target = _penultimateValue?.Target;
            if (target == null)
                return default;

            return new BindingPathLastMember(Path, target, _members[_members.Length - 1]);
        }

        protected override IBindingEventListener GetSourceListener()
        {
            return this;
        }

        protected override void OnListenerAdded(IBindingPathObserverListener listener)
        {
            UpdateIfNeed();
            if (_observable && _listeners[_listeners.Length - 1] == null && _penultimateValue != null)
            {
                var target = _penultimateValue.Target;
                if (target != null)
                    _listeners[_listeners.Length - 1] = _members[_members.Length - 1].TryObserve(target, GetLastMemberListener(), null) ?? Default.Disposable;
            }
        }

        protected override void OnDisposed()
        {
            ClearListeners();
            _penultimateValue?.Release();
            _penultimateValue = null;
            _members = null;
            _exception = null;
        }

        private void UpdateIfNeed()
        {
            if (!HasSourceListener)
            {
                AddSourceListener();
                Update();
            }
        }

        private bool Update()
        {
            try
            {
                if (!TryGetSourceValue(out var source))
                    return false;

                if (source == null)
                {
                    SetMembers(null, null, null);
                    return true;
                }

                ClearListeners();

                if (_hasStablePath && _members != null)
                {
                    UpdateHasStablePath(source);
                    return true;
                }

                var paths = Path.Parts;
                var members = new IBindingMemberInfo[paths.Length];
                for (var i = 0; i < members.Length - 1; i++)
                {
                    var member = GetBindingMember(source.GetType(), paths[i], _ignoreAttachedMembers);
                    if (member == null)
                    {
                        if (_optional)
                            SetMembers(null, null, null);
                        else
                            BindingExceptionManager.ThrowInvalidBindingMember(source.GetType(), paths[i]);
                        return true;
                    }

                    members[i] = member;
                    if (_observable)
                        _listeners[i] = member.TryObserve(source, this, null);

                    source = member.GetValue(source, null, null);
                    if (source.IsNullOrUnsetValue())
                    {
                        SetMembers(null, null, null);
                        return true;
                    }
                }

                if (_observable && HasListeners)
                    _listeners[_listeners.Length - 1] = members[members.Length - 1].TryObserve(source, GetLastMemberListener(), null) ?? Default.Disposable;
                SetMembers(Service<IWeakReferenceProvider>.Instance.GetWeakReference(source, Default.Metadata), members, null);
            }
            catch (Exception e)
            {
                SetMembers(null, null, e);
                OnError(e);
            }

            return true;
        }

        private void UpdateHasStablePath(object source)
        {
            for (var index = 0; index < _members.Length - 1; index++)
            {
                var member = _members[index];
                if (_observable)
                    _listeners[index] = member.TryObserve(source, this, null);

                source = member.GetValue(source, null, null);
                if (source.IsNullOrUnsetValue())
                {
                    SetMembers(null, _members, null);
                    return;
                }
            }

            if (_observable && HasListeners)
                _listeners[_listeners.Length - 1] = _members[_members.Length - 1].TryObserve(source, GetLastMemberListener(), null) ?? Default.Disposable;

            SetMembers(Service<IWeakReferenceProvider>.Instance.GetWeakReference(source, Default.Metadata), _members, null);
        }

        private void SetMembers(IWeakReference? penultimateValue, IBindingMemberInfo[] members, Exception exception)
        {
            _penultimateValue = penultimateValue;
            _members = members;
            _exception = exception;
            OnPathMembersChanged();
        }

        private IBindingEventListener GetLastMemberListener()
        {
            if (_lastMemberListener == null)
                _lastMemberListener = new LastMemberListener(this);
            return _lastMemberListener;
        }

        private void ClearListeners()
        {
            if (_listeners != null)
            {
                for (var index = 0; index < _listeners.Length; index++)
                {
                    _listeners[index]?.Dispose();
                    _listeners[index] = null;
                }
            }
        }

        #endregion

        #region Nested types

        private sealed class LastMemberListener : IBindingEventListener
        {
            #region Fields

            private readonly MultiPathObserver _observer;

            #endregion

            #region Constructors

            public LastMemberListener(MultiPathObserver observer)
            {
                _observer = observer;
            }

            #endregion

            #region Properties

            public bool IsAlive => _observer.IsAlive;

            public bool IsWeak => true;

            #endregion

            #region Implementation of interfaces

            public bool TryHandle(object sender, object message)
            {
                _observer.OnLastMemberChanged();
                return true;
            }

            #endregion
        }

        #endregion
    }
}