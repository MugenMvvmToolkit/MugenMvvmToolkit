using System;
using System.Runtime.CompilerServices;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Observers
{
    public class MultiPathObserver : ObserverBase, IEventListener, IValueHolder<IWeakReference>//todo optimize observable vs non-observable
    {
        #region Fields

        private readonly Unsubscriber[]? _listeners;
        protected readonly BindingMemberFlags MemberFlags;
        private Exception? _exception;
        private IEventListener? _lastMemberListener;
        private IBindingMemberInfo[]? _members;
        private IWeakReference? _penultimateValue;

        private byte _state;

        #endregion

        #region Constructors

        public MultiPathObserver(object target, IMemberPath path,
            BindingMemberFlags memberFlags, bool hasStablePath, bool observable, bool optional)
            : base(target)
        {
            MemberFlags = memberFlags;
            if (hasStablePath)
                _state |= HasStablePathFlag;
            if (optional)
                _state |= OptionalFlag;
            if (observable)
                _listeners = new Unsubscriber[path.Members.Length];
            Path = path;
        }

        #endregion

        #region Properties

        public override IMemberPath Path { get; }

        public bool IsWeak => false;

        IWeakReference? IValueHolder<IWeakReference>.Value { get; set; }

        protected bool HasStablePath => CheckFlag(HasStablePathFlag);

        protected bool Optional => CheckFlag(OptionalFlag);

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

        bool IEventListener.TryHandle(object sender, object? message)
        {
            return Update();
        }

        #endregion

        #region Methods

        public override MemberPathMembers GetMembers(IReadOnlyMetadataContext? metadata = null)
        {
            UpdateIfNeed();
            if (_exception != null)
                return new MemberPathMembers(_exception);

            var target = Target;
            if (target == null)
                return default;

            var penultimateValue = _penultimateValue?.Target;
            if (penultimateValue == null || _members == null)
                return default;

            return new MemberPathMembers(target, _members);
        }

        public override MemberPathLastMember GetLastMember(IReadOnlyMetadataContext? metadata = null)
        {
            UpdateIfNeed();
            if (_exception != null)
                return new MemberPathLastMember(_exception);

            var target = _penultimateValue?.Target;
            if (target == null || _members == null)
                return default;

            return new MemberPathLastMember(target, _members[_members.Length - 1]);
        }

        protected override void OnListenerAdded(IMemberPathObserverListener listener)
        {
            UpdateIfNeed();
            if (_listeners != null && _members != null && _listeners[_listeners.Length - 1].IsEmpty && _penultimateValue != null)
            {
                var target = _penultimateValue.Target;
                if (target != null)
                    SubscribeLastMember(target, _members[_members.Length - 1]);
            }
        }

        protected override void OnListenersRemoved()
        {
            UnsubscribeLastMember();
        }

        protected override void OnDisposed()
        {
            ClearListeners();
            UnsubscribeLastMember();
            this.ReleaseWeakReference();
            _penultimateValue = null;
            _members = null;
            _exception = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
                var target = Target;
                if (target == null)
                {
                    SetMembers(null, null, null);
                    return false;
                }

                ClearListeners();

                if (HasStablePath && _members != null)
                {
                    UpdateHasStablePath(_members, target);
                    return true;
                }

                var paths = Path.Members;
                var members = new IBindingMemberInfo[paths.Length];
                var provider = MugenBindingService.MemberProvider;
                var lastIndex = members.Length - 1;
                var memberFlags = MemberFlags;
                var type = GetTargetType(target, memberFlags);
                for (var i = 0; i < members.Length; i++)
                {
                    var member = provider.GetMember(type, paths[i],
                        i == lastIndex ? BindingMemberType.Field | BindingMemberType.Property : BindingMemberType.Field | BindingMemberType.Property | BindingMemberType.Event,
                        memberFlags);
                    if (i == 1)
                        memberFlags &= ~BindingMemberFlags.Static;
                    if (member == null)
                    {
                        if (Optional)
                            SetMembers(null, null, null);
                        else
                            BindingExceptionManager.ThrowInvalidBindingMember(target.GetType(), paths[i]);
                        return true;
                    }

                    members[i] = member;
                    if (i == lastIndex)
                        break;

                    if (_listeners != null && member is IObservableBindingMemberInfo observable)
                        _listeners[i] = observable.TryObserve(target, this);

                    target = (member as IBindingMemberAccessorInfo)?.GetValue(target);
                    if (target.IsNullOrUnsetValue())
                    {
                        SetMembers(null, null, null);
                        return true;
                    }

                    type = target!.GetType();
                }

                if (_listeners != null && HasListeners)
                    SubscribeLastMember(target, members[members.Length - 1]);
                SetMembers(target.ToWeakReference(), members, null);
            }
            catch (Exception e)
            {
                SetMembers(null, null, e);
                OnError(e);
            }

            return true;
        }

        private void UpdateHasStablePath(IBindingMemberInfo[] members, object target)
        {
            for (var index = 0; index < members.Length - 1; index++)
            {
                var member = members[index];
                if (_listeners != null && member is IObservableBindingMemberInfo observable)
                    _listeners[index] = observable.TryObserve(target, this);

                target = (member as IBindingMemberAccessorInfo)?.GetValue(target)!;
                if (target.IsNullOrUnsetValue())
                {
                    SetMembers(null, members, null);
                    return;
                }
            }

            if (_listeners != null && HasListeners && _members != null)
                SubscribeLastMember(target, _members[_members.Length - 1]);

            SetMembers(target.ToWeakReference(), members, null);
        }

        private void SetMembers(IWeakReference? penultimateValue, IBindingMemberInfo[]? members, Exception? exception)
        {
            _penultimateValue = penultimateValue;
            _members = members;
            _exception = exception;
            OnPathMembersChanged();
        }

        protected IEventListener GetLastMemberListener()
        {
            if (_lastMemberListener == null)
                _lastMemberListener = new LastMemberListener(this.ToWeakReference());
            return _lastMemberListener;
        }

        private void ClearListeners()
        {
            if (_listeners != null)
            {
                for (var index = 0; index < _listeners.Length; index++)
                {
                    _listeners[index].Unsubscribe();
                    _listeners[index] = default;
                }

                UnsubscribeLastMember();
            }
        }

        protected virtual void SubscribeLastMember(object target, IBindingMemberInfo? lastMember)
        {
            if (_listeners == null)
                return;
            Unsubscriber unsubscriber = default;
            if (lastMember is IObservableBindingMemberInfo observable)
                unsubscriber = observable.TryObserve(target, GetLastMemberListener());
            if (unsubscriber.IsEmpty)
                _listeners[_listeners.Length - 1] = Unsubscriber.NoDoUnsubscriber;
            else
                _listeners[_listeners.Length - 1] = unsubscriber;
        }

        protected virtual void UnsubscribeLastMember()
        {
            if (_listeners == null)
                return;

            var listener = _listeners[_listeners.Length - 1];
            if (!listener.IsEmpty)
            {
                listener.Unsubscribe();
                _listeners![_listeners.Length - 1] = default;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool CheckFlag(byte flag)
        {
            return (_state & flag) == flag;
        }

        #endregion

        #region Nested types

        private sealed class LastMemberListener : IEventListener
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