using System;
using MugenMvvm.Enums;
using MugenMvvm.Infrastructure.Messaging;
using MugenMvvm.Interfaces.Messaging;

// ReSharper disable once CheckNamespace
namespace MugenMvvm
{
    public static partial class MugenExtensions
    {
        #region Methods

        public static IMessengerSubscriber SubscribeWeak<TTarget, TMessage>(this IMessenger messenger, TTarget target, Action<TTarget, object, TMessage, IMessengerContext> action,
            ThreadExecutionMode executionMode)
            where TTarget : class
        {
            Should.NotBeNull(messenger, nameof(messenger));
            var subscriber = new WeakDelegateMessengerSubscriber<TTarget, TMessage>(target, action);
            messenger.Subscribe(subscriber, executionMode);
            return subscriber;
        }

        public static IMessengerSubscriber SubscribeWeak<TMessage>(this IMessenger messenger, Action<object, TMessage, IMessengerContext> action, ThreadExecutionMode executionMode)
        {
            Should.NotBeNull(messenger, nameof(messenger));
            var subscriber = new WeakDelegateMessengerSubscriber<object, TMessage>(action);
            messenger.Subscribe(subscriber, executionMode);
            return subscriber;
        }

        public static IMessengerSubscriber Subscribe<TMessage>(this IMessenger messenger, Action<object, TMessage, IMessengerContext> action, ThreadExecutionMode executionMode)
        {
            Should.NotBeNull(messenger, nameof(messenger));
            var subscriber = new DelegateMessengerSubscriber<TMessage>(action);
            messenger.Subscribe(subscriber, executionMode);
            return subscriber;
        }

        public static IMessengerSubscriber Subscribe(this IMessenger messenger, IMessengerHandler handler, ThreadExecutionMode executionMode)
        {
            Should.NotBeNull(messenger, nameof(messenger));
            var subscriber = new MessengerHandlerSubscriber(handler);
            messenger.Subscribe(subscriber, executionMode);
            return subscriber;
        }

        public static void UnsubscribeAll(this IMessenger messenger)
        {
            Should.NotBeNull(messenger, nameof(messenger));
            foreach (var subscriber in messenger.GetSubscribers())
                messenger.Unsubscribe(subscriber.Subscriber);
        }

        #endregion
    }
}