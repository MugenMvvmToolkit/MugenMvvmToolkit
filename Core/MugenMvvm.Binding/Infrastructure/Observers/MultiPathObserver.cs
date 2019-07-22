using System;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Infrastructure.Observers
{
    public sealed class MultiPathObserver : ObserverBase, IBindingEventListener, IWeakReferenceHolder
    {
        #region Fields

        private byte _state;
        private readonly IDisposable?[]? _listeners;
        private Exception? _exception;
        private IBindingEventListener? _lastMemberListener;
        private IBindingMemberInfo[]? _members;
        private IWeakReference? _penultimateValue;

        #endregion

        #region Constructors

        public MultiPathObserver(IWeakReference source, IBindingPath path,
            bool ignoreAttachedMembers, bool hasStablePath, bool observable, bool optional)
            : base(source)
        {
            if (ignoreAttachedMembers)
                _state |= IgnoreAttachedMembersFlag;
            if (hasStablePath)
                _state |= HasStablePathFlag;
            if (optional)
                _state |= OptionalFlag;
            if (observable)
                _listeners = new IDisposable[path.Parts.Length];
            Path = path;
        }

        #endregion

        #region Properties

        public override IBindingPath Path { get; }

        public bool IsWeak => false;

        public IWeakReference? WeakReference { get; set; }

        private bool IgnoreAttachedMembers => CheckFlag(IgnoreAttachedMembersFlag);

        private bool HasStablePath => CheckFlag(HasStablePathFlag);

        private bool Optional => CheckFlag(OptionalFlag);

        private bool IsInitialized
        {
            get => CheckFlag(InitializedFlag);
            set
            {
                if (value)
                    _state |= InitializedFlag;
            }
        }

        #endregion

        #region Implementation of interfaces

        bool IBindingEventListener.TryHandle(object sender, object? message)
        {
            return Update();
        }

        #endregion

        #region Methods

        public override BindingPathMembers GetMembers(IReadOnlyMetadataContext? metadata = null)
        {
            UpdateIfNeed();
            if (_exception != null)
                return new BindingPathMembers(Path, _exception);

            var source = Source;
            if (source == null)
                return default;

            var target = _penultimateValue?.Target;
            if (target == null || _members == null)
                return default;

            return new BindingPathMembers(Path, source, target, _members, _members[_members.Length - 1]);
        }

        public override BindingPathLastMember GetLastMember(IReadOnlyMetadataContext? metadata = null)
        {
            UpdateIfNeed();
            if (_exception != null)
                return new BindingPathLastMember(Path, _exception);

            var target = _penultimateValue?.Target;
            if (target == null || _members == null)
                return default;

            return new BindingPathLastMember(Path, target, _members[_members.Length - 1]);
        }

        protected override void OnListenerAdded(IBindingPathObserverListener listener)
        {
            UpdateIfNeed();
            if (_listeners != null && _members != null && _listeners[_listeners.Length - 1] == null && _penultimateValue != null)
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
            if (!IsInitialized)
            {
                IsInitialized = true;
                Update();
            }
        }

        private bool Update()
        {
            try
            {
                var source = Source;
                if (source == null)
                {
                    SetMembers(null, null, null);
                    return false;
                }

                ClearListeners();

                if (HasStablePath && _members != null)
                {
                    UpdateHasStablePath(_members, source);
                    return true;
                }

                var paths = Path.Parts;
                var members = new IBindingMemberInfo[paths.Length];
                for (var i = 0; i < members.Length - 1; i++)
                {
                    var member = GetBindingMember(source!.GetType(), paths[i], IgnoreAttachedMembers);
                    if (member == null)
                    {
                        if (Optional)
                            SetMembers(null, null, null);
                        else
                            BindingExceptionManager.ThrowInvalidBindingMember(source.GetType(), paths[i]);
                        return true;
                    }

                    members[i] = member;
                    if (_listeners != null)
                        _listeners[i] = member.TryObserve(source, this, null);

                    source = member.GetValue(source, null, null);
                    if (source.IsNullOrUnsetValue())
                    {
                        SetMembers(null, null, null);
                        return true;
                    }
                }

                if (_listeners != null && HasListeners)
                    _listeners[_listeners.Length - 1] = members[members.Length - 1].TryObserve(source, GetLastMemberListener(), null) ?? Default.Disposable;
                SetMembers(Service<IWeakReferenceProvider>.Instance.GetWeakReference(source), members, null);
            }
            catch (Exception e)
            {
                SetMembers(null, null, e);
                OnError(e);
            }

            return true;
        }

        private void UpdateHasStablePath(IBindingMemberInfo[] members, object source)
        {
            for (var index = 0; index < members.Length - 1; index++)
            {
                var member = members[index];
                if (_listeners != null)
                    _listeners[index] = member.TryObserve(source, this, null);

                source = member.GetValue(source, null, null)!;
                if (source.IsNullOrUnsetValue())
                {
                    SetMembers(null, members, null);
                    return;
                }
            }

            if (_listeners != null && HasListeners)
                _listeners[_listeners.Length - 1] = members[members.Length - 1].TryObserve(source, GetLastMemberListener(), null) ?? Default.Disposable;

            SetMembers(Service<IWeakReferenceProvider>.Instance.GetWeakReference(source), members, null);
        }

        private void SetMembers(IWeakReference? penultimateValue, IBindingMemberInfo[]? members, Exception? exception)
        {
            _penultimateValue = penultimateValue;
            _members = members;
            _exception = exception;
            OnPathMembersChanged();
        }

        private IBindingEventListener GetLastMemberListener()
        {
            if (_lastMemberListener == null)
                _lastMemberListener = new LastMemberListener(Service<IWeakReferenceProvider>.Instance.GetWeakReference(this));
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

        private bool CheckFlag(byte flag)
        {
            return (_state & flag) == flag;
        }

        #endregion

        #region Nested types

        private sealed class LastMemberListener : IBindingEventListener
        {
            #region Fields

            private readonly IWeakReference _observer;

            #endregion

            #region Constructors

            public LastMemberListener(IWeakReference observer)
            {
                _observer = observer;
            }

            #endregion

            #region Properties

            public bool IsAlive => _observer.Target != null;

            public bool IsWeak => true;

            #endregion

            #region Implementation of interfaces

            public bool TryHandle(object sender, object? message)
            {
                var observer = (MultiPathObserver?)_observer.Target;
                if (observer == null)
                    return false;
                observer.OnLastMemberChanged();
                return true;
            }

            #endregion
        }

        #endregion
    }
}