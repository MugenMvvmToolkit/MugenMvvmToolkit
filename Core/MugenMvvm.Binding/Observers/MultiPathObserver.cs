using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.Binding.Observers
{
    public class MultiPathObserver : MultiPathObserverBase
    {
        #region Fields

        private readonly ActionToken[] _listeners;
        private IEventListener? _lastMemberListener;

        #endregion

        #region Constructors

        public MultiPathObserver(object target, IMemberPath path, MemberFlags memberFlags, bool hasStablePath, bool optional)
            : base(target, path, memberFlags, hasStablePath, optional)
        {
            _listeners = new ActionToken[path.Members.Length];
        }

        #endregion

        #region Methods

        protected IEventListener GetLastMemberListener()
        {
            if (_lastMemberListener == null)
                _lastMemberListener = new LastMemberListener(this.ToWeakReference());
            return _lastMemberListener;
        }

        protected override void OnListenerAdded(IMemberPathObserverListener listener)
        {
            base.OnListenerAdded(listener);
            if (Members != null && _listeners[_listeners.Length - 1].IsEmpty && PenultimateValueOrException is IWeakReference penultimateRef)
            {
                var target = penultimateRef.Target;
                if (target != null)
                    SubscribeLastMember(target, Members[Members.Length - 1]);
            }
        }

        protected override void SubscribeMember(int index, object target, IObservableMemberInfo member)
        {
            _listeners[index] = member.TryObserve(target, this);
        }

        protected override void SubscribeLastMember(object target, IMemberInfo? lastMember)
        {
            ActionToken unsubscriber = default;
            if (lastMember is IObservableMemberInfo observable)
                unsubscriber = observable.TryObserve(target, GetLastMemberListener());
            if (unsubscriber.IsEmpty)
                _listeners[_listeners.Length - 1] = ActionToken.NoDoToken;
            else
                _listeners[_listeners.Length - 1] = unsubscriber;
        }

        protected override void UnsubscribeLastMember()
        {
            _listeners[_listeners.Length - 1].Dispose();
        }

        protected override void ClearListeners()
        {
            for (var index = 0; index < _listeners.Length; index++)
                _listeners[index].Dispose();

            UnsubscribeLastMember();
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