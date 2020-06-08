using System;
using System.Runtime.CompilerServices;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Observers.PathObservers
{
    public abstract class MultiPathObserverBase : ObserverBase, IEventListener, IValueHolder<IWeakReference>
    {
        #region Fields

        protected readonly MemberFlags MemberFlags;
        protected IMemberInfo[]? Members;
        protected object? PenultimateValueOrException;
        private byte _state;

        #endregion

        #region Constructors

        protected MultiPathObserverBase(object target, IMemberPath path, MemberFlags memberFlags, bool hasStablePath, bool optional)
            : base(target)
        {
            MemberFlags = memberFlags;
            if (hasStablePath)
                _state |= HasStablePathFlag;
            if (optional)
                _state |= OptionalFlag;
            Path = path;
        }

        #endregion

        #region Properties

        public override IMemberPath Path { get; }

        public bool IsWeak => false;

        IWeakReference? IValueHolder<IWeakReference>.Value { get; set; }

        protected bool HasStablePath => CheckFlag(HasStablePathFlag);

        protected bool Optional => CheckFlag(OptionalFlag);

        #endregion

        #region Implementation of interfaces

        bool IEventListener.TryHandle<T>(object? sender, in T message, IReadOnlyMetadataContext? metadata)
        {
            return Update();
        }

        #endregion

        #region Methods

        protected abstract void SubscribeMember(int index, object? target, IObservableMemberInfo member, IReadOnlyMetadataContext? metadata);

        protected abstract void SubscribeLastMember(object? target, IMemberInfo? lastMember, IReadOnlyMetadataContext? metadata);

        protected abstract void UnsubscribeLastMember();

        protected abstract void ClearListeners();

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
                var penultimateValue = penultimateRef.Target;
                var members = Members;
                if (penultimateValue == null || members == null)
                    return default;

                return new MemberPathLastMember(penultimateValue, members[members.Length - 1]);
            }

            if (PenultimateValueOrException is Exception e)
                return new MemberPathLastMember(e);
            return default;
        }

        protected override void OnListenersAdded()
        {
            UpdateIfNeed();
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
                _state |= UpdatingFlag;
                var target = Target;
                if (target == null)
                {
                    SetMembers(null, null, null);
                    return false;
                }

                ClearListeners();

                if (HasStablePath && Members != null)
                {
                    UpdateHasStablePath(Members, MemberFlags.HasFlagEx(MemberFlags.Static) ? null : target);
                    return true;
                }

                var paths = Path.Members;
                var members = new IMemberInfo[paths.Count];
                var memberManager = MugenBindingService.MemberManager;
                var lastIndex = members.Length - 1;
                var memberFlags = MemberFlags;
                var type = memberFlags.GetTargetType(ref target);
                var metadata = TryGetMetadata();
                for (var i = 0; i < members.Length; i++)
                {
                    if (i == 1)
                        memberFlags = memberFlags.SetInstanceOrStaticFlags(false);
                    var member = memberManager.GetMember(type, i == lastIndex ? MemberType.Accessor | MemberType.Event : MemberType.Accessor, memberFlags, paths[i], metadata);
                    if (member == null)
                    {
                        if (Optional)
                            SetMembers(null, null, null);
                        else
                            BindingExceptionManager.ThrowInvalidBindingMember(type, paths[i]);
                        return true;
                    }

                    members[i] = member;
                    if (i == lastIndex)
                        break;

                    if (member is IObservableMemberInfo observable)
                        SubscribeMember(i, target, observable, metadata);

                    target = (member as IMemberAccessorInfo)?.GetValue(target, metadata);
                    if (target.IsNullOrUnsetValue())
                    {
                        SetMembers(null, null, null);
                        return true;
                    }

                    type = target.GetType();
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
                _state = (byte)(_state & ~UpdatingFlag);
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

                target = (member as IMemberAccessorInfo)?.GetValue(target, metadata)!;
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
            PenultimateValueOrException = (object?)exception ?? penultimateValue;
            Members = members;
            if (exception == null)
                _state |= InitializedFlag;
            OnPathMembersChanged();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool CheckFlag(byte flag)
        {
            return (_state & flag) == flag;
        }

        #endregion
    }
}