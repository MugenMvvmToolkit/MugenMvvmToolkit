using System;
using System.Runtime.CompilerServices;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Observers
{
    public sealed class SinglePathObserver : ObserverBase, IEventListener, IWeakReferenceHolder
    {
        #region Fields

        private byte _state;
        private Exception? _exception;
        private IBindingPropertyInfo? _lastMember;
        private IDisposable? _lastMemberUnsubscriber;

        #endregion

        #region Constructors

        public SinglePathObserver(IWeakReference source, IMemberPath path, bool ignoreAttachedMembers, bool observable, bool optional)
            : base(source)
        {
            Should.NotBeNull(path, nameof(path));
            if (ignoreAttachedMembers)
                _state |= IgnoreAttachedMembersFlag;
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

        public IWeakReference? WeakReference { get; set; }

        private bool IgnoreAttachedMembers => CheckFlag(IgnoreAttachedMembersFlag);

        private bool Observable => CheckFlag(ObservableFlag);

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
                return new MemberPathMembers(Path, _exception);
            var target = Source;
            if (target == null || _lastMember == null)
                return default;
            return new MemberPathMembers(Path, target, target, null, _lastMember);
        }

        public override MemberPathLastMember GetLastMember(IReadOnlyMetadataContext? metadata = null)
        {
            UpdateIfNeed();
            if (_exception != null)
                return new MemberPathLastMember(Path, _exception);
            var target = Source;
            if (target == null || _lastMember == null)
                return default;
            return new MemberPathLastMember(Path, target, _lastMember);
        }

        protected override void OnListenerAdded(IMemberPathObserverListener listener)
        {
            UpdateIfNeed();
            if (Observable && _lastMemberUnsubscriber == null && _lastMember != null)
            {
                var source = Source;
                if (source == null)
                    _lastMemberUnsubscriber = Default.Disposable;
                else
                    Subscribe(source, _lastMember);
            }
        }

        protected override void OnListenersRemoved()
        {
            if (_lastMemberUnsubscriber != null)
            {
                _lastMemberUnsubscriber?.Dispose();
                _lastMemberUnsubscriber = null;
            }
        }

        protected override void OnDisposed()
        {
            _lastMemberUnsubscriber?.Dispose();
            _lastMemberUnsubscriber = null;
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
                var source = Source;
                if (source == null)
                {
                    SetLastMember(null, null);
                    return;
                }

                if (_lastMember != null)
                    return;

                _lastMember = GetMember(source.GetType(), Path.Path, IgnoreAttachedMembers);
                if (_lastMember == null)
                {
                    if (Optional)
                        SetLastMember(null, null);
                    else
                        BindingExceptionManager.ThrowInvalidBindingMember(source.GetType(), Path.Path);
                    return;
                }

                if (Observable && HasListeners)
                    Subscribe(source, _lastMember);
                SetLastMember(_lastMember, _exception);
            }
            catch (Exception e)
            {
                SetLastMember(null, e);
                OnError(e);
            }
        }

        private void SetLastMember(IBindingPropertyInfo? lastMember, Exception? exception)
        {
            _lastMember = lastMember;
            _exception = exception;
            OnLastMemberChanged();
        }

        private void Subscribe(object source, IBindingPropertyInfo? lastMember)
        {
            _lastMemberUnsubscriber?.Dispose();
            _lastMemberUnsubscriber = lastMember?.TryObserve(source, this) ?? Default.Disposable;
        }

        private bool CheckFlag(byte flag)
        {
            return (_state & flag) == flag;
        }

        #endregion
    }
}