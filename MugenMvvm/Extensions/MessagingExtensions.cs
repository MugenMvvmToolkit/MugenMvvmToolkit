using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Messaging.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;
using MugenMvvm.Messaging;

namespace MugenMvvm.Extensions
{
    public static partial class MugenExtensions
    {
        public static IMessageContext Publish(this IMessagePublisher publisher, object message, IReadOnlyMetadataContext? metadata = null) =>
            publisher.Publish(null, message, metadata);

        public static IMessageContext Publish(this IMessagePublisher publisher, object? sender, object message, IReadOnlyMetadataContext? metadata = null)
        {
            var messageContext = publisher.GetMessageContext(sender, message, metadata);
            publisher.Publish(messageContext);
            return messageContext;
        }

        public static ActionToken Subscribe(this IMessenger messenger, object subscriber, ThreadExecutionMode? executionMode = null, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(messenger, nameof(messenger));
            Should.NotBeNull(subscriber, nameof(subscriber));
            if (!messenger.TrySubscribe(subscriber, executionMode, metadata))
                ExceptionManager.ThrowRequestNotSupported<IMessengerSubscriberComponent>(messenger, subscriber, metadata);
            return ActionToken.FromDelegate((m, h) => ((IMessenger) m!).TryUnsubscribe(h!), messenger, subscriber);
        }

        public static ActionToken Subscribe<TMessage>(this IMessenger messenger, Action<object?, TMessage, IMessageContext> action, ThreadExecutionMode? executionMode = null,
            IReadOnlyMetadataContext? metadata = null) =>
            messenger.Subscribe(new DelegateMessengerHandler<TMessage>(action), executionMode, metadata);

        public static ActionToken SubscribeWeak(this IMessenger messenger, IMessengerHandlerBase subscriber, ThreadExecutionMode? executionMode = null,
            IReadOnlyMetadataContext? metadata = null) =>
            messenger.Subscribe(subscriber.ToWeakReference(), executionMode, metadata);

        public static ActionToken SubscribeWeak<TTarget, TMessage>(this IMessenger messenger, TTarget target, Action<TTarget, object?, TMessage, IMessageContext> action,
            ThreadExecutionMode? executionMode = null, IReadOnlyMetadataContext? metadata = null)
            where TTarget : class =>
            messenger.Subscribe(new WeakDelegateMessengerHandler<TTarget, TMessage>(target, action), executionMode, metadata);

        public static ActionToken SubscribeWeak<TMessage>(this IMessenger messenger, Action<object?, TMessage, IMessageContext> action, ThreadExecutionMode? executionMode = null,
            IReadOnlyMetadataContext? metadata = null) =>
            messenger.Subscribe(new WeakDelegateMessengerHandler<object, TMessage>(action), executionMode, metadata);
    }
}