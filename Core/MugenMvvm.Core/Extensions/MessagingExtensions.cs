using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Messaging;
using MugenMvvm.Messaging.Components;

// ReSharper disable once CheckNamespace
namespace MugenMvvm
{
    public static partial class MugenExtensions
    {
        #region Methods

        public static IMessageContext Publish(this IMessagePublisher publisher, object message, IMetadataContext? metadata = null)
        {
            return publisher.Publish(null, message, metadata);
        }

        public static IMessageContext Publish(this IMessagePublisher publisher, object? sender, object message, IMetadataContext? metadata = null)
        {
            Should.NotBeNull(publisher, nameof(publisher));
            var context = publisher.GetMessageContext(sender, message, metadata);
            publisher.Publish(context);
            return context;
        }

        public static MessengerHandlerComponent.IMessengerSubscriber SubscribeWeak<TTarget, TMessage>(this IMessenger messenger, TTarget target,
            Action<TTarget, TMessage, IMessageContext> action,
            ThreadExecutionMode? executionMode = null, IReadOnlyMetadataContext? metadata = null)
            where TTarget : class
        {
            Should.NotBeNull(messenger, nameof(messenger));
            var subscriber = new WeakDelegateMessengerSubscriber<TTarget, TMessage>(target, action);
            messenger.Subscribe(subscriber, executionMode, metadata);
            return subscriber;
        }

        public static MessengerHandlerComponent.IMessengerSubscriber SubscribeWeak<TMessage>(this IMessenger messenger, Action<TMessage, IMessageContext> action,
            ThreadExecutionMode? executionMode = null, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(messenger, nameof(messenger));
            var subscriber = new WeakDelegateMessengerSubscriber<object, TMessage>(action);
            messenger.Subscribe(subscriber, executionMode, metadata);
            return subscriber;
        }

        public static MessengerHandlerComponent.IMessengerSubscriber Subscribe<TMessage>(this IMessenger messenger, Action<TMessage, IMessageContext> action,
            ThreadExecutionMode? executionMode = null, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(messenger, nameof(messenger));
            var subscriber = new DelegateMessengerSubscriber<TMessage>(action);
            messenger.Subscribe(subscriber, executionMode, metadata);
            return subscriber;
        }

        public static MessengerHandlerComponent.IMessengerSubscriber Subscribe(this IMessenger messenger, IMessengerHandler handler,
            ThreadExecutionMode? executionMode = null, IReadOnlyMetadataContext? metadata = null)
        {
            return messenger.Subscribe(handler, false, executionMode, metadata);
        }

        public static MessengerHandlerComponent.IMessengerSubscriber SubscribeWeak(this IMessenger messenger, IMessengerHandler handler,
            ThreadExecutionMode? executionMode = null, IReadOnlyMetadataContext? metadata = null)
        {
            return messenger.Subscribe(handler, true, executionMode, metadata);
        }

        private static MessengerHandlerComponent.IMessengerSubscriber Subscribe(this IMessenger messenger, IMessengerHandler handler, bool isWeak,
            ThreadExecutionMode? executionMode, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(messenger, nameof(messenger));
            var subscriber = new MessengerHandlerSubscriber(handler, isWeak);
            messenger.Subscribe(subscriber, executionMode, metadata);
            return subscriber;
        }

        public static void UnsubscribeAll(this IMessenger messenger, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(messenger, nameof(messenger));
            var list = messenger.GetSubscribers();
            for (var index = 0; index < list.Count; index++)
                messenger.Unsubscribe(list[index].Subscriber, metadata);
        }

        #endregion
    }
}