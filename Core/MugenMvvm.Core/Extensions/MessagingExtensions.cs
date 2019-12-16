using System;
using System.Reflection;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Messaging;
using MugenMvvm.Messaging.Components;
using MugenMvvm.Messaging.Subscribers;

namespace MugenMvvm.Extensions
{
    public static partial class MugenExtensions
    {
        #region Methods

        public static IMessageContext Publish(this IMessagePublisher publisher, object message, IReadOnlyMetadataContext? metadata = null)
        {
            return publisher.Publish(null, message, metadata);
        }

        public static IMessageContext Publish(this IMessagePublisher publisher, object? sender, object message, IReadOnlyMetadataContext? metadata = null)
        {
            var messageContext = publisher.GetMessageContext(sender, message, metadata);
            publisher.Publish(messageContext);
            return messageContext;
        }

        public static MessengerHandlerComponent.IMessengerSubscriber SubscribeWeak<TTarget, TMessage>(this IMessenger messenger, TTarget target,
            Action<TTarget, TMessage, IMessageContext> action,
            ThreadExecutionMode? executionMode = null, IReadOnlyMetadataContext? metadata = null)
            where TTarget : class
        {
            Should.NotBeNull(messenger, nameof(messenger));
            var subscriber = new WeakDelegateMessengerSubscriber<TTarget, TMessage>(target.ToWeakReference(), action);
            messenger.Subscribe(subscriber, executionMode, metadata);
            return subscriber;
        }

        public static MessengerHandlerComponent.IMessengerSubscriber SubscribeWeak<TMessage>(this IMessenger messenger, Action<TMessage, IMessageContext> action,
            ThreadExecutionMode? executionMode = null, IReadOnlyMetadataContext? metadata = null, IReflectionDelegateProvider? reflectionDelegateProvider = null)
        {
            Should.NotBeNull(messenger, nameof(messenger));
            Should.NotBeNull(action, nameof(action));
            Should.BeSupported(action.Target != null, MessageConstant.StaticDelegateCannotBeWeak);
            Should.BeSupported(!action.Target!.GetType().IsAnonymousClass(), MessageConstant.AnonymousDelegateCannotBeWeak);
            var methodInvoker = action.GetMethodInfo().GetMethodInvoker<Action<object, TMessage, IMessageContext>>(reflectionDelegateProvider);
            var subscriber = new WeakDelegateMessengerSubscriber<object, TMessage>(action.Target.ToWeakReference(), methodInvoker);
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
            ThreadExecutionMode? executionMode = null, IReadOnlyMetadataContext? metadata = null, IReflectionDelegateProvider? reflectionDelegateProvider = null)
        {
            return messenger.Subscribe(handler, false, executionMode, metadata, reflectionDelegateProvider);
        }

        public static MessengerHandlerComponent.IMessengerSubscriber SubscribeWeak(this IMessenger messenger, IMessengerHandler handler,
            ThreadExecutionMode? executionMode = null, IReadOnlyMetadataContext? metadata = null, IReflectionDelegateProvider? reflectionDelegateProvider = null)
        {
            return messenger.Subscribe(handler, true, executionMode, metadata, reflectionDelegateProvider);
        }

        private static MessengerHandlerComponent.IMessengerSubscriber Subscribe(this IMessenger messenger, IMessengerHandler handler, bool isWeak,
            ThreadExecutionMode? executionMode, IReadOnlyMetadataContext? metadata, IReflectionDelegateProvider? reflectionDelegateProvider)
        {
            Should.NotBeNull(messenger, nameof(messenger));
            var subscriber = new MessengerHandlerSubscriber(handler, isWeak, reflectionDelegateProvider);
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