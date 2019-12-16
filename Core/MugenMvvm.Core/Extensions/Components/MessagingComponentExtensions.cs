using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Messaging.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Extensions.Components
{
    public static class MessagingComponentExtensions
    {
        #region Methods

        public static IMessageContext? TryGetMessageContext(this IMessageContextProviderComponent[] components, object? sender, object message, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(message, nameof(message));
            for (var i = 0; i < components.Length; i++)
            {
                var ctx = components[i].TryGetMessageContext(sender, message, metadata);
                if (ctx != null)
                    return ctx;
            }

            return null;
        }

        public static void OnSubscribed(this IMessengerSubscriberListener[] listeners, IMessenger messenger, object subscriber, ThreadExecutionMode executionMode, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(messenger, nameof(messenger));
            Should.NotBeNull(subscriber, nameof(subscriber));
            Should.NotBeNull(executionMode, nameof(executionMode));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnSubscribed(messenger, subscriber, executionMode, metadata);
        }

        public static void OnUnsubscribed(this IMessengerSubscriberListener[] listeners, IMessenger messenger, object subscriber, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(messenger, nameof(messenger));
            Should.NotBeNull(subscriber, nameof(subscriber));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnUnsubscribed(messenger, subscriber, metadata);
        }

        public static bool CanHandle(this IMessengerHandlerComponent[] components, object subscriber, Type messageType)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(subscriber, nameof(subscriber));
            Should.NotBeNull(messageType, nameof(messageType));
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].CanHandle(subscriber, messageType))
                    return true;
            }

            return false;
        }

        public static MessengerResult? TryHandle(this IMessengerHandlerComponent[] components, object subscriber, IMessageContext messageContext)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(subscriber, nameof(subscriber));
            Should.NotBeNull(messageContext, nameof(messageContext));
            for (var i = 0; i < components.Length; i++)
            {
                var result = components[i].TryHandle(subscriber, messageContext);
                if (result != null)
                    return result;
            }

            return null;
        }

        public static void OnHandling(this IMessengerHandlerListener[] listeners, IMessenger messenger, object subscriber, IMessageContext messageContext)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(messenger, nameof(messenger));
            Should.NotBeNull(subscriber, nameof(subscriber));
            Should.NotBeNull(messageContext, nameof(messageContext));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnHandling(messenger, subscriber, messageContext);
        }

        public static void OnHandled(this IMessengerHandlerListener[] listeners, IMessenger messenger, MessengerResult? result, object subscriber, IMessageContext messageContext)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(messenger, nameof(messenger));
            Should.NotBeNull(subscriber, nameof(subscriber));
            Should.NotBeNull(messageContext, nameof(messageContext));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnHandled(messenger, result, subscriber, messageContext);
        }

        #endregion
    }
}