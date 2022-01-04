using System;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Observation.Observers
{
    public class MultiPathObserver : MultiPathObserverBase
    {
        private readonly ActionToken[] _listeners;
        private IEventListener? _lastMemberListener;

        public MultiPathObserver(object target, IMemberPath path, EnumFlags<MemberFlags> memberFlags, bool hasStablePath, bool optional, bool isWeak)
            : base(target, path, memberFlags, hasStablePath, optional, isWeak)
        {
            _listeners = new ActionToken[path.Members.Count];
        }

        protected override (bool, Exception?) OnListenersAdded()
        {
            var result = base.OnListenersAdded();
            if (State.members != null && _listeners[_listeners.Length - 1].IsEmpty && State.penultimateValueOrException is IWeakReference penultimateRef)
            {
                var target = penultimateRef.Target;
                if (target != null)
                    SubscribeLastMember(target, State.members[State.members.Length - 1], TryGetMetadata());
            }

            return result;
        }

        protected override void SubscribeMember(int index, object? target, IObservableMemberInfo member, IReadOnlyMetadataContext? metadata) =>
            _listeners[index] = member.TryObserve(target, this, metadata);

        protected override void SubscribeLastMember(object? target, IMemberInfo? lastMember, IReadOnlyMetadataContext? metadata)
        {
            ActionToken unsubscriber = default;
            if (lastMember != null && lastMember.MemberType != MemberType.Event && lastMember is IObservableMemberInfo observable)
                unsubscriber = observable.TryObserve(target, GetLastMemberListener(), metadata);
            if (unsubscriber.IsEmpty)
                _listeners[_listeners.Length - 1] = ActionToken.NoDo;
            else
                _listeners[_listeners.Length - 1] = unsubscriber;
        }

        protected override void UnsubscribeLastMember() => _listeners[_listeners.Length - 1].Dispose();

        protected override void ClearListeners()
        {
            for (var index = 0; index < _listeners.Length; index++)
                _listeners[index].Dispose();

            UnsubscribeLastMember();
        }

        protected IEventListener GetLastMemberListener() => _lastMemberListener ??= IsWeak ? new LastMemberListenerWeak(this.ToWeakReference()) : new LastMemberListener(this);

        private sealed class LastMemberListener : IWeakEventListener
        {
            private readonly MultiPathObserver _observer;

            public LastMemberListener(MultiPathObserver observer)
            {
                _observer = observer;
            }

            public bool IsWeak => true;

            public bool IsAlive => true;

            public bool TryHandle(object? sender, object? message, IReadOnlyMetadataContext? metadata)
            {
                _observer.OnLastMemberChanged();
                return true;
            }
        }

        private sealed class LastMemberListenerWeak : IWeakEventListener
        {
            private readonly IWeakReference _observer;

            public LastMemberListenerWeak(IWeakReference observer)
            {
                _observer = observer;
            }

            public bool IsWeak => true;

            public bool IsAlive => _observer.IsAlive;

            public bool TryHandle(object? sender, object? message, IReadOnlyMetadataContext? metadata)
            {
                var observer = (MultiPathObserver?)_observer.Target;
                if (observer == null)
                    return false;
                observer.OnLastMemberChanged();
                return true;
            }
        }
    }
}