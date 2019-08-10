using System;
using System.Reflection;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Messaging;

namespace MugenMvvm.Messaging
{
    public sealed class WeakDelegateMessengerSubscriber<TTarget, TMessage> : IMessengerSubscriber
            where TTarget : class
    {
        #region Fields

        private readonly Action<TTarget, object, TMessage, IMessengerContext> _action;
        private readonly IWeakReference _reference;

        #endregion

        #region Constructors

        public WeakDelegateMessengerSubscriber(TTarget target, Action<TTarget, object, TMessage, IMessengerContext> action)
        {
            Should.NotBeNull(target, nameof(target));
            Should.NotBeNull(action, nameof(action));
            _reference = target.ToWeakReference();
            _action = action;
        }

        public WeakDelegateMessengerSubscriber(Action<object, TMessage, IMessengerContext> action)
        {
            Should.NotBeNull(action, nameof(action));
            Should.BeSupported(action.Target != null, MessageConstants.StaticDelegateCannotBeWeak);
            Should.BeSupported(!action.Target.GetType().IsAnonymousClass(), MessageConstants.AnonymousDelegateCannotBeWeak);
            _reference = action.Target.ToWeakReference();
            _action = action.GetMethodInfo().GetMethodInvoker<Action<TTarget, object, TMessage, IMessengerContext>>();
        }

        #endregion

        #region Implementation of interfaces

        public bool Equals(IMessengerSubscriber other)
        {
            return ReferenceEquals(other, this);
        }

        public MessengerSubscriberResult Handle(object sender, object message, IMessengerContext messengerContext)
        {
            var target = (TTarget?)_reference.Target;
            if (target == null)
                return MessengerSubscriberResult.Invalid;
            if (message is TMessage m)
            {
                _action(target, sender, m, messengerContext);
                return MessengerSubscriberResult.Handled;
            }

            return MessengerSubscriberResult.Ignored;
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            return _reference.Target?.ToString() ?? base.ToString();
        }

        #endregion
    }
}