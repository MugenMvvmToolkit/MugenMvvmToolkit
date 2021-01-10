using System;
using System.Collections.Generic;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Messaging.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;
using MugenMvvm.Messaging;

namespace MugenMvvm.Extensions.Components
{
    public static class MessagingComponentExtensions
    {
        #region Methods

        public static IMessageContext? TryGetMessageContext(this IMessageContextProviderComponent[] components, IMessenger messenger, object? sender, object message, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(messenger, nameof(messenger));
            Should.NotBeNull(message, nameof(message));
            for (var i = 0; i < components.Length; i++)
            {
                var ctx = components[i].TryGetMessageContext(messenger, sender, message, metadata);
                if (ctx != null)
                    return ctx;
            }

            return null;
        }

        public static bool TryPublish(this IMessagePublisherComponent[] components, IMessenger messenger, IMessageContext messageContext)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(messenger, nameof(messenger));
            Should.NotBeNull(messageContext, nameof(messageContext));
            var published = false;
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].TryPublish(messenger, messageContext))
                    published = true;
            }

            return published;
        }

        public static bool TrySubscribe(this IMessengerSubscriberComponent[] components, IMessenger messenger, object subscriber, ThreadExecutionMode? executionMode, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(messenger, nameof(messenger));
            Should.NotBeNull(subscriber, nameof(subscriber));
            var result = false;
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].TrySubscribe(messenger, subscriber, executionMode, metadata))
                    result = true;
            }

            return result;
        }

        public static bool TryUnsubscribe(this IMessengerSubscriberComponent[] components, IMessenger messenger, object subscriber, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(messenger, nameof(messenger));
            Should.NotBeNull(subscriber, nameof(subscriber));
            var result = false;
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].TryUnsubscribe(messenger, subscriber, metadata))
                    result = true;
            }

            return result;
        }

        public static bool TryUnsubscribeAll(this IMessengerSubscriberComponent[] components, IMessenger messenger, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(messenger, nameof(messenger));
            var result = false;
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].TryUnsubscribeAll(messenger, metadata))
                    result = true;
            }

            return result;
        }

        public static ItemOrIReadOnlyList<MessengerSubscriberInfo> TryGetSubscribers(this IMessengerSubscriberComponent[] components, IMessenger messenger,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(messenger, nameof(messenger));
            if (components.Length == 1)
                return components[0].TryGetSubscribers(messenger, metadata);
            var subscribers = new ItemOrListEditor<MessengerSubscriberInfo>();
            for (var i = 0; i < components.Length; i++)
                subscribers.AddRange(components[i].TryGetSubscribers(messenger, metadata));
            return subscribers.ToItemOrList();
        }

        public static ItemOrIReadOnlyList<MessengerHandler> TryGetMessengerHandlers(this IMessengerSubscriberComponent[] components, IMessenger messenger, Type messageType, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(messenger, nameof(messenger));
            Should.NotBeNull(messageType, nameof(messageType));
            if (components.Length == 1)
                return components[0].TryGetMessengerHandlers(messenger, messageType, metadata);
            var handlers = new ItemOrListEditor<MessengerHandler>();
            for (var i = 0; i < components.Length; i++)
                handlers.AddRange(components[i].TryGetMessengerHandlers(messenger, messageType, metadata));
            return handlers.ToItemOrList();
        }

        #endregion
    }
}