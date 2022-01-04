using System;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Metadata;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Observation.Observers
{
    public abstract class MultiPathObserverBase : ObserverBase, IWeakEventListener, IValueHolder<IWeakReference>
    {
        protected (IMemberInfo[]? members, object? penultimateValueOrException) State;

        protected MultiPathObserverBase(object target, IMemberPath path, EnumFlags<MemberFlags> memberFlags, bool hasStablePath, bool optional, bool isWeak)
            : base(target, memberFlags)
        {
            if (hasStablePath)
                SetFlag(HasStablePathFlag);
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
            var state = State;
            if (state.members != null)
            {
                var target = Target;
                if (target == null)
                    return default;

                return new MemberPathMembers(target, state.members);
            }

            if (state.penultimateValueOrException is Exception e)
                return new MemberPathMembers(e);
            return default;
        }

        public override MemberPathLastMember GetLastMember(IReadOnlyMetadataContext? metadata = null)
        {
            if (!CheckFlag(InitializedFlag))
                UpdateIfNeed();
            var state = State;
            var members = state.members;
            if (members != null)
            {
                var penultimateValue = state.penultimateValueOrException;
                if (penultimateValue is IWeakReference weakReference)
                {
                    penultimateValue = weakReference.Target;
                    if (penultimateValue == null)
                        return default;
                }
                else if (penultimateValue == BindingMetadata.UnsetValue)
                    return default;

                return new MemberPathLastMember(penultimateValue, members[members.Length - 1]);
            }

            if (state.penultimateValueOrException is Exception e)
                return new MemberPathLastMember(e);
            return default;
        }

        protected abstract void SubscribeMember(int index, object? target, IObservableMemberInfo member, IReadOnlyMetadataContext? metadata);

        protected abstract void SubscribeLastMember(object? target, IMemberInfo? lastMember, IReadOnlyMetadataContext? metadata);

        protected abstract void UnsubscribeLastMember();

        protected abstract void ClearListeners();

        protected override (bool, Exception?) OnListenersAdded()
        {
            if (CheckFlag(InitializedFlag) || CheckFlag(UpdatingFlag))
                return default;

            var result = Update();
            return (result.raise, result.raise ? State.penultimateValueOrException as Exception : null);
        }

        protected override void RaiseOnListenersAdded() => Raise(null, true);

        protected override void OnListenersRemoved() => UnsubscribeLastMember();

        protected override void OnDisposed()
        {
            ClearListeners();
            UnsubscribeLastMember();
            this.ReleaseWeakReference();
            State = default;
        }

        private static object? GetPenultimateValueRef(object? value) => value is null or ValueType ? value : value.ToWeakReference();

        private void UpdateIfNeed()
        {
            Exception? exception;
            (bool, bool) raise;
            lock (this)
            {
                if (!CheckFlag(InitializedFlag) && !CheckFlag(UpdatingFlag))
                {
                    raise = Update();
                    exception = State.penultimateValueOrException as Exception;
                }
                else
                {
                    raise = default;
                    exception = null;
                }
            }

            Raise(exception, raise.Item2);
        }

        private void Raise(Exception? exception, bool raise)
        {
            if (exception != null)
                OnError(exception);
            if (raise)
            {
                OnPathMembersChanged();
                lock (this)
                {
                    ClearFlag(UpdatingFlag);
                }
            }
        }

        private (bool isValid, bool raise) Update()
        {
            try
            {
                SetFlag(UpdatingFlag);
                var target = Target;
                if (target == null)
                {
                    SetMembers(null, null, null);
                    return (false, true);
                }

                ClearListeners();

                if (HasStablePath && State.members != null)
                {
                    UpdateHasStablePath(State.members, MemberFlags.HasFlag(Enums.MemberFlags.Static) ? null : target);
                    return (true, true);
                }

                var paths = Path.Members;
                var members = new IMemberInfo[paths.Count];
                var memberManager = MugenService.MemberManager;
                var lastIndex = members.Length - 1;
                var memberFlags = MemberFlags;
                var type = memberFlags.GetTargetType(ref target);
                var metadata = TryGetMetadata();
                for (var i = 0; i < members.Length; i++)
                {
                    if (i == 1)
                        memberFlags = memberFlags.SetInstanceOrStaticFlags(false);
                    var member = memberManager.TryGetMember(type, i == lastIndex ? MemberType.Accessor | MemberType.Event : MemberType.Accessor, memberFlags, paths[i], metadata);
                    if (member == null || target == null && !member.MemberFlags.HasFlag(Enums.MemberFlags.Static) && !member.MemberFlags.HasFlag(Enums.MemberFlags.Extension))
                    {
                        if (Optional || target == null && i != 0)
                            SetMembers(null, null, null);
                        else
                            ExceptionManager.ThrowInvalidBindingMember(type, paths[i]);
                        return (true, true);
                    }

                    members[i] = member;
                    if (i == lastIndex)
                        break;

                    if (member is IObservableMemberInfo observable)
                        SubscribeMember(i, target, observable, metadata);

                    target = (member as IAccessorMemberInfo)?.GetValue(target, metadata);
                    if (target.IsUnsetValue())
                    {
                        SetMembers(null, null, null);
                        return (true, true);
                    }

                    type = target?.GetType() ?? member.Type;
                }

                SetMembers(GetPenultimateValueRef(target), members, null);
                if (HasListeners)
                    SubscribeLastMember(target, members[members.Length - 1], metadata);
            }
            catch (Exception e)
            {
                SetMembers(null, null, e);
            }

            return (true, true);
        }

        private void UpdateHasStablePath(IMemberInfo[] members, object? target)
        {
            var metadata = TryGetMetadata();
            for (var index = 0; index < members.Length - 1; index++)
            {
                var member = members[index];
                if (member is IObservableMemberInfo observable)
                    SubscribeMember(index, target, observable, metadata);

                target = (member as IAccessorMemberInfo)?.GetValue(target, metadata)!;
                if (target.IsNullOrUnsetValue())
                {
                    SetMembers(BindingMetadata.UnsetValue, members, null);
                    return;
                }
            }

            SetMembers(GetPenultimateValueRef(target), members, null);
            if (HasListeners)
                SubscribeLastMember(target, members[members.Length - 1], metadata);
        }

        private void SetMembers(object? penultimateValue, IMemberInfo[]? members, Exception? exception)
        {
            State = (members, (object?) exception ?? penultimateValue);
            if (exception == null)
                SetFlag(InitializedFlag);
        }

        bool IEventListener.TryHandle(object? sender, object? message, IReadOnlyMetadataContext? metadata)
        {
            (bool isValid, bool raise) state;
            Exception? exception;
            lock (this)
            {
                state = Update();
                if (!state.isValid)
                    return false;
                exception = State.penultimateValueOrException as Exception;
            }

            Raise(exception, state.raise);
            return true;
        }
    }
}