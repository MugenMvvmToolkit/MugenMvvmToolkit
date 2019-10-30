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
    public class SinglePathObserver : ObserverBase, IEventListener, IValueHolder<IWeakReference>//todo opt
    {
        #region Fields

        protected readonly BindingMemberFlags MemberFlags;

        private Exception? _exception;
        private IBindingMemberInfo? _lastMember;
        private Unsubscriber _lastMemberUnsubscriber;

        private byte _state;

        #endregion

        #region Constructors

        public SinglePathObserver(object target, IMemberPath path, BindingMemberFlags memberFlags, bool observable, bool optional)
            : base(target)
        {
            Should.NotBeNull(path, nameof(path));
            MemberFlags = memberFlags;
            if (observable)
                _state |= ObservableFlag;
            if (optional)
                _state |= OptionalFlag;
            Path = path;
        }

        #endregion

        #region Properties

        public override IMemberPath Path { get; }

        public bool IsWeak => false;

        IWeakReference? IValueHolder<IWeakReference>.Value { get; set; }

        protected bool Observable => CheckFlag(ObservableFlag);

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
            OnLastMemberChanged();
            return true;
        }

        #endregion

        #region Methods

        public override MemberPathMembers GetMembers(IReadOnlyMetadataContext? metadata = null)
        {
            UpdateIfNeed();
            if (_exception != null)
                return new MemberPathMembers(_exception);

            var target = Target;
            if (target == null || _lastMember == null)
                return default;

            return new MemberPathMembers(target, new[] { _lastMember });
        }

        public override MemberPathLastMember GetLastMember(IReadOnlyMetadataContext? metadata = null)
        {
            UpdateIfNeed();
            if (_exception != null)
                return new MemberPathLastMember(_exception);

            var target = Target;
            if (target == null || _lastMember == null)
                return default;

            return new MemberPathLastMember(target, _lastMember);
        }

        protected override void OnListenerAdded(IMemberPathObserverListener listener)
        {
            UpdateIfNeed();
            if (Observable && _lastMemberUnsubscriber.IsEmpty && _lastMember != null)
            {
                var target = Target;
                if (target == null)
                    _lastMemberUnsubscriber = Unsubscriber.NoDoUnsubscriber;
                else
                    SubscribeLastMember(target, _lastMember);
            }
        }

        protected override void OnListenersRemoved()
        {
            UnsubscribeLastMember();
        }

        protected override void OnDisposed()
        {
            UnsubscribeLastMember();
            this.ReleaseWeakReference();
            _lastMember = null;
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

        private void Update()
        {
            try
            {
                var target = Target;
                if (target == null)
                {
                    SetLastMember(null, null);
                    return;
                }

                if (_lastMember != null)
                    return;

                _lastMember = MugenBindingService
                    .MemberProvider
                    .GetMember(GetTargetType(target, MemberFlags), Path.Path, BindingMemberType.Event | BindingMemberType.Field | BindingMemberType.Property, MemberFlags);
                if (_lastMember == null)
                {
                    if (Optional)
                        SetLastMember(null, null);
                    else
                        BindingExceptionManager.ThrowInvalidBindingMember(target.GetType(), Path.Path);
                    return;
                }

                if (Observable && HasListeners)
                    SubscribeLastMember(target, _lastMember);
                SetLastMember(_lastMember, _exception);
            }
            catch (Exception e)
            {
                SetLastMember(null, e);
                OnError(e);
            }
        }

        private void SetLastMember(IBindingMemberInfo? lastMember, Exception? exception)
        {
            _lastMember = lastMember;
            _exception = exception;
            OnLastMemberChanged();
        }

        protected virtual void SubscribeLastMember(object target, IBindingMemberInfo? lastMember)
        {
            _lastMemberUnsubscriber.Unsubscribe();
            if (lastMember is IObservableBindingMemberInfo observable)
                _lastMemberUnsubscriber = observable.TryObserve(target, this);
            if (_lastMemberUnsubscriber.IsEmpty)
                _lastMemberUnsubscriber = Unsubscriber.NoDoUnsubscriber;
        }

        protected virtual void UnsubscribeLastMember()
        {
            _lastMemberUnsubscriber.Unsubscribe();
            _lastMemberUnsubscriber = default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool CheckFlag(byte flag)
        {
            return (_state & flag) == flag;
        }

        #endregion
    }
}