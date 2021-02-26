using System;
using System.Runtime.CompilerServices;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Observation.Observers
{
    public abstract class MultiPathObserverBase : ObserverBase, IEventListener, IValueHolder<IWeakReference>
    {
        protected IMemberInfo[]? Members;
        protected object? PenultimateValueOrException;

        protected MultiPathObserverBase(object target, IMemberPath path, EnumFlags<MemberFlags> memberFlags, bool hasStablePath, bool optional)
            : base(target, memberFlags)
        {
            if (hasStablePath)
                SetFlag(HasStablePathFlag);
            if (optional)
                SetFlag(OptionalFlag);
            Path = path;
        }

        public override IMemberPath Path { get; }

        IWeakReference? IValueHolder<IWeakReference>.Value { get; set; }

        public override MemberPathMembers GetMembers(IReadOnlyMetadataContext? metadata = null)
        {
            UpdateIfNeed();
            if (PenultimateValueOrException is IWeakReference)
            {
                var target = Target;
                var members = Members;
                if (target == null || members == null)
                    return default;

                return new MemberPathMembers(target, members);
            }

            if (PenultimateValueOrException is Exception e)
                return new MemberPathMembers(e);
            return default;
        }

        public override MemberPathLastMember GetLastMember(IReadOnlyMetadataContext? metadata = null)
        {
            UpdateIfNeed();
            if (PenultimateValueOrException is IWeakReference penultimateRef)
            {
                var members = Members;
                if (members == null)
                    return default;

                var penultimateValue = penultimateRef.Target;
                var member = members[members.Length - 1];
                if (penultimateValue == null && !member.MemberFlags.HasFlag(Enums.MemberFlags.Extension))
                    return default;

                return new MemberPathLastMember(penultimateValue, member);
            }

            if (PenultimateValueOrException is Exception e)
                return new MemberPathLastMember(e);
            return default;
        }

        protected abstract void SubscribeMember(int index, object? target, IObservableMemberInfo member, IReadOnlyMetadataContext? metadata);

        protected abstract void SubscribeLastMember(object? target, IMemberInfo? lastMember, IReadOnlyMetadataContext? metadata);

        protected abstract void UnsubscribeLastMember();

        protected abstract void ClearListeners();

        protected override void OnListenersAdded() => UpdateIfNeed();

        protected override void OnListenersRemoved() => UnsubscribeLastMember();

        protected override void OnDisposed()
        {
            ClearListeners();
            UnsubscribeLastMember();
            this.ReleaseWeakReference();
            PenultimateValueOrException = null;
            Members = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateIfNeed()
        {
            if (!CheckFlag(InitializedFlag) && !CheckFlag(UpdatingFlag))
                Update();
        }

        private bool Update()
        {
            try
            {
                SetFlag(UpdatingFlag);
                var target = Target;
                if (target == null)
                {
                    SetMembers(null, null, null);
                    return false;
                }

                ClearListeners();

                if (HasStablePath && Members != null)
                {
                    UpdateHasStablePath(Members, MemberFlags.HasFlag(Enums.MemberFlags.Static) ? null : target);
                    return true;
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
                        return true;
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
                        return true;
                    }

                    type = target?.GetType() ?? member.Type;
                }

                if (HasListeners)
                    SubscribeLastMember(target, members[members.Length - 1], metadata);
                SetMembers(target.ToWeakReference(), members, null);
            }
            catch (Exception e)
            {
                SetMembers(null, null, e);
                OnError(e);
            }
            finally
            {
                ClearFlag(UpdatingFlag);
            }

            return true;
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
                    SetMembers(null, members, null);
                    return;
                }
            }

            if (HasListeners && Members != null)
                SubscribeLastMember(target, Members[Members.Length - 1], metadata);

            SetMembers(target.ToWeakReference(), members, null);
        }

        private void SetMembers(IWeakReference? penultimateValue, IMemberInfo[]? members, Exception? exception)
        {
            PenultimateValueOrException = (object?) exception ?? penultimateValue;
            Members = members;
            if (exception == null)
                SetFlag(InitializedFlag);
            OnPathMembersChanged();
        }

        bool IEventListener.TryHandle(object? sender, object? message, IReadOnlyMetadataContext? metadata) => Update();
    }
}