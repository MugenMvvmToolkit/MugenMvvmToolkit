using System;
using System.Reflection;
using MugenMvvm.Interfaces;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Models;

namespace MugenMvvm.Infrastructure.Messaging
{
    public class WeakDelegateMessengerSubscriber<TTarget, TMessage> : IMessengerSubscriber
        where TTarget : class
    {
        #region Fields

        private readonly Action<TTarget, object, TMessage, IMessengerContext> _action;
        private readonly WeakReference _reference;

        #endregion

        #region Constructors

        public WeakDelegateMessengerSubscriber(TTarget target, Action<TTarget, object, TMessage, IMessengerContext> action)
        {
            Should.NotBeNull(target, nameof(target));
            Should.NotBeNull(action, nameof(action));
            _reference = MugenExtensions.GetWeakReference(target);
            _action = action;
        }

        public WeakDelegateMessengerSubscriber(Action<object, TMessage, IMessengerContext> action)
        {
            Should.NotBeNull(action, nameof(action));
            Should.BeSupported(action.Target != null, MessageConstants.StaticDelegateCannotBeWeak);
            Should.BeSupported(!action.Target.GetType().IsAnonymousClass(), MessageConstants.AnonymousDelegateCannotBeWeak);
            _reference = MugenExtensions.GetWeakReference(action.Target);
            _action = Singleton<IReflectionManager>.Instance.GetMethodDelegate<Action<TTarget, object, TMessage, IMessengerContext>>(action.GetMethodInfo());
        }

        #endregion

        #region Implementation of interfaces

        public bool Equals(IMessengerSubscriber other)
        {
            return ReferenceEquals(other, this);
        }

        public SubscriberResult Handle(object sender, object message, IMessengerContext messengerContext)
        {
            var target = (TTarget)_reference.Target;
            if (target == null)
                return SubscriberResult.Invalid;
            if (message is TMessage m)
            {
                _action(target, sender, m, messengerContext);
                return SubscriberResult.Handled;
            }

            return SubscriberResult.Ignored;
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            return _reference.Target?.ToString();
        }

        #endregion
    }
}