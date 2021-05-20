using System;
using System.Runtime.CompilerServices;
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
    public class SinglePathObserver : ObserverBase, IEventListener, IValueHolder<IWeakReference>
    {
        private object? _lastMemberOrException;
        private ActionToken _lastMemberUnsubscriber;

        public SinglePathObserver(object target, IMemberPath path, EnumFlags<MemberFlags> memberFlags, bool optional)
            : base(target, memberFlags)
        {
            Should.NotBeNull(path, nameof(path));
            if (optional)
                SetFlag(OptionalFlag);
            Path = path;
        }

        public override IMemberPath Path { get; }

        IWeakReference? IValueHolder<IWeakReference>.Value { get; set; }

        public override MemberPathMembers GetMembers(IReadOnlyMetadataContext? metadata = null)
        {
            UpdateIfNeed();
            if (_lastMemberOrException is IMemberInfo member)
            {
                var target = Target;
                if (target == null)
                    return default;
                return new MemberPathMembers(target, new ItemOrIReadOnlyList<IMemberInfo>(member, true));
            }

            if (_lastMemberOrException is Exception e)
                return new MemberPathMembers(e);
            return default;
        }

        public override MemberPathLastMember GetLastMember(IReadOnlyMetadataContext? metadata = null)
        {
            UpdateIfNeed();
            if (_lastMemberOrException is IMemberInfo member)
            {
                var target = Target;
                if (target == null)
                    return default;
                return new MemberPathLastMember(target, member);
            }

            if (_lastMemberOrException is Exception e)
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

        protected override void OnListenersAdded()
        {
            UpdateIfNeed();
            if (_lastMemberUnsubscriber.IsEmpty && _lastMemberOrException is IMemberInfo lastMember)
            {
                var target = Target;
                if (target == null)
                    _lastMemberUnsubscriber = ActionToken.NoDo;
                else
                    SubscribeLastMember(target, lastMember, TryGetMetadata());
            }
        }

        protected override void OnListenersRemoved() => UnsubscribeLastMember();

        protected override void OnDisposed()
        {
            UnsubscribeLastMember();
            this.ReleaseWeakReference();
            _lastMemberOrException = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateIfNeed()
        {
            if (!CheckFlag(InitializedFlag) && !CheckFlag(UpdatingFlag))
                Update();
        }

        private void Update()
        {
            try
            {
                SetFlag(UpdatingFlag);
                var target = Target;
                if (target == null)
                {
                    SetLastMember(null, null);
                    return;
                }

                if (_lastMemberOrException is IMemberInfo)
                    return;

                var metadata = TryGetMetadata();
                var targetType = MemberFlags.GetTargetType(ref target);
                var lastMember = MugenService
                                 .MemberManager
                                 .TryGetMember(targetType, MemberType.Event | MemberType.Accessor, MemberFlags, Path.Path, metadata);
                if (lastMember == null)
                {
                    if (Optional)
                        SetLastMember(null, null);
                    else
                        ExceptionManager.ThrowInvalidBindingMember(targetType, Path.Path);
                    return;
                }

                if (HasListeners)
                    SubscribeLastMember(target, lastMember, metadata);
                SetLastMember(lastMember, null);
            }
            catch (Exception e)
            {
                SetLastMember(null, e);
                OnError(e);
            }
            finally
            {
                ClearFlag(UpdatingFlag);
            }
        }

        private void SetLastMember(IMemberInfo? lastMember, Exception? exception)
        {
            _lastMemberOrException = (object?) exception ?? lastMember;
            if (exception == null)
                SetFlag(InitializedFlag);
            OnLastMemberChanged();
        }

        bool IEventListener.TryHandle(object? sender, object? message, IReadOnlyMetadataContext? metadata)
        {
            OnLastMemberChanged();
            return true;
        }
    }
}