using System;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Observation.Observers
{
    public class SinglePathObserver : ObserverBase, IWeakEventListener, IValueHolder<IWeakReference>
    {
        private object? _lastMemberOrException;
        private ActionToken _lastMemberUnsubscriber;

        public SinglePathObserver(object target, IMemberPath path, EnumFlags<MemberFlags> memberFlags, bool optional, bool isWeak)
            : base(target, memberFlags)
        {
            Should.NotBeNull(path, nameof(path));
            if (optional)
                SetFlag(OptionalFlag);
            if (isWeak)
                SetFlag(WeakFlag);
            Path = path;
        }

        public override IMemberPath Path { get; }

        IWeakReference? IValueHolder<IWeakReference>.Value { get; set; }

        public override MemberPathMembers GetMembers(IReadOnlyMetadataContext? metadata = null)
        {
            if (!CheckFlag(InitializedFlag))
                UpdateIfNeed();
            var lastMemberOrException = _lastMemberOrException;
            if (lastMemberOrException is IMemberInfo member)
            {
                var target = Target;
                if (target == null)
                    return default;
                return new MemberPathMembers(target, new ItemOrIReadOnlyList<IMemberInfo>(member));
            }

            if (lastMemberOrException is Exception e)
                return new MemberPathMembers(e);
            return default;
        }

        public override MemberPathLastMember GetLastMember(IReadOnlyMetadataContext? metadata = null)
        {
            if (!CheckFlag(InitializedFlag))
                UpdateIfNeed();
            var lastMemberOrException = _lastMemberOrException;
            if (lastMemberOrException is IMemberInfo member)
            {
                var target = Target;
                if (target == null)
                    return default;
                return new MemberPathLastMember(target, member);
            }

            if (lastMemberOrException is Exception e)
                return new MemberPathLastMember(e);
            return default;
        }

        protected virtual void SubscribeLastMember(object? target, IMemberInfo? lastMember, IReadOnlyMetadataContext? metadata)
        {
            _lastMemberUnsubscriber.Dispose();
            if (lastMember != null && lastMember.MemberType != MemberType.Event && lastMember is IObservableMemberInfo observable)
                _lastMemberUnsubscriber = observable.TryObserve(target, this, metadata);
            if (_lastMemberUnsubscriber.IsEmpty)
                _lastMemberUnsubscriber = ActionToken.NoDo;
        }

        protected virtual void UnsubscribeLastMember() => _lastMemberUnsubscriber.Dispose();

        protected override (bool, Exception?) OnListenersAdded()
        {
            var raise = !CheckFlag(InitializedFlag) && !CheckFlag(UpdatingFlag) && Update();
            if (_lastMemberUnsubscriber.IsEmpty && _lastMemberOrException is IMemberInfo lastMember)
            {
                var target = Target;
                if (target == null)
                    _lastMemberUnsubscriber = ActionToken.NoDo;
                else
                    SubscribeLastMember(target, lastMember, TryGetMetadata());
            }

            return (raise, raise ? _lastMemberOrException as Exception : null);
        }

        protected override void RaiseOnListenersAdded() => Raise(null, true);

        protected override void OnListenersRemoved() => UnsubscribeLastMember();

        protected override void OnDisposed()
        {
            UnsubscribeLastMember();
            this.ReleaseWeakReference();
            _lastMemberOrException = null;
        }

        private void UpdateIfNeed()
        {
            Exception? exception;
            bool raise;
            lock (this)
            {
                if (!CheckFlag(InitializedFlag) && !CheckFlag(UpdatingFlag))
                {
                    raise = Update();
                    exception = _lastMemberOrException as Exception;
                }
                else
                {
                    raise = false;
                    exception = null;
                }
            }

            if (raise)
                Raise(exception, true);
        }

        private void Raise(Exception? exception, bool raise)
        {
            if (exception != null)
                OnError(exception);
            if (raise)
            {
                OnLastMemberChanged();
                lock (this)
                {
                    ClearFlag(UpdatingFlag);
                }
            }
        }

        private bool Update()
        {
            try
            {
                var target = Target;
                if (target == null)
                {
                    SetLastMember(null, null);
                    return true;
                }

                if (_lastMemberOrException is IMemberInfo)
                    return false;

                SetFlag(UpdatingFlag);
                var metadata = TryGetMetadata();
                var targetType = MemberFlags.GetTargetType(ref target);
                var lastMember = MugenService.MemberManager.TryGetMember(targetType, MemberType.Event | MemberType.Accessor, MemberFlags, Path.Path, metadata);
                if (lastMember == null)
                {
                    if (Optional)
                        SetLastMember(null, null);
                    else
                        ExceptionManager.ThrowInvalidBindingMember(targetType, Path.Path);
                    return true;
                }

                SetLastMember(lastMember, null);
                if (HasListeners)
                    SubscribeLastMember(target, lastMember, metadata);
            }
            catch (Exception e)
            {
                SetLastMember(null, e);
            }

            return true;
        }

        private void SetLastMember(IMemberInfo? lastMember, Exception? exception)
        {
            _lastMemberOrException = (object?)exception ?? lastMember;
            if (exception == null)
                SetFlag(InitializedFlag);
        }

        bool IEventListener.TryHandle(object? sender, object? message, IReadOnlyMetadataContext? metadata)
        {
            OnLastMemberChanged();
            return true;
        }
    }
}