﻿using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Observation.Observers
{
    public sealed class MethodEmptyPathObserver : ObserverBase, IEventListener, IValueHolder<IWeakReference>
    {
        private readonly string _method;
        private ActionToken _unsubscriber;

        public MethodEmptyPathObserver(string method, object target, EnumFlags<MemberFlags> memberFlags) : base(target, memberFlags)
        {
            Should.NotBeNull(method, nameof(method));
            _method = method;
            IsDisposable = true;
        }

        public override IMemberPath Path => MemberPath.Empty;

        IWeakReference? IValueHolder<IWeakReference>.Value { get; set; }

        public override MemberPathMembers GetMembers(IReadOnlyMetadataContext? metadata = null)
        {
            var target = Target;
            if (target == null)
                return default;

            return new MemberPathMembers(target, ConstantMemberInfo.Target);
        }

        public override MemberPathLastMember GetLastMember(IReadOnlyMetadataContext? metadata = null)
        {
            var target = Target;
            if (target == null)
                return default;

            return new MemberPathLastMember(target, ConstantMemberInfo.Target);
        }

        protected override void OnDisposed()
        {
            OnListenersRemoved();
            this.ReleaseWeakReference();
        }

        protected override void OnListenersAdded()
        {
            if (!_unsubscriber.IsEmpty)
                return;
            var target = Target;
            if (target == null)
                _unsubscriber = ActionToken.NoDo;
            else
            {
                var member = MugenService.MemberManager.TryGetMember(MemberFlags.GetTargetType(ref target), MemberType.Method, MemberFlags, _method, TryGetMetadata());
                if (member is IObservableMemberInfo observable)
                    _unsubscriber = observable.TryObserve(target, this, TryGetMetadata());
                if (_unsubscriber.IsEmpty)
                    _unsubscriber = ActionToken.NoDo;
            }
        }

        protected override void OnListenersRemoved() => _unsubscriber.Dispose();

        bool IEventListener.TryHandle(object? sender, object? message, IReadOnlyMetadataContext? metadata)
        {
            OnLastMemberChanged();
            return true;
        }
    }
}