using System;
using System.Runtime.CompilerServices;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Messaging.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Messaging;

namespace MugenMvvm.Extensions.Components
{
    public static class MessagingComponentExtensions
    {
        public static IMessageContext? TryGetMessageContext(this ItemOrArray<IMessageContextProviderComponent> components, IMessenger messenger, object? sender, object message,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(messenger, nameof(messenger));
            Should.NotBeNull(message, nameof(message));
            foreach (var c in components)
            {
                var ctx = c.TryGetMessageContext(messenger, sender, message, metadata);
                if (ctx != null)
                    return ctx;
            }

            return null;
        }
        
        public static bool TryPublish(this ItemOrArray<IMessagePublisherComponent> components, IMessenger messenger, IMessageContext messageContext)
        {
            Should.NotBeNull(messenger, nameof(messenger));
            Should.NotBeNull(messageContext, nameof(messageContext));
            var published = false;
            foreach (var c in components)
            {
                if (c.TryPublish(messenger, messageContext))
                    published = true;
            }

            return published;
        }

        public static bool TrySubscribe(this ItemOrArray<IMessengerSubscriberComponent> components, IMessenger messenger, object subscriber, ThreadExecutionMode? executionMode,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(messenger, nameof(messenger));
            Should.NotBeNull(subscriber, nameof(subscriber));
            var result = false;
            foreach (var c in components)
            {
                if (c.TrySubscribe(messenger, subscriber, executionMode, metadata))
                    result = true;
            }

            return result;
        }

        public static bool TryUnsubscribe(this ItemOrArray<IMessengerSubscriberComponent> components, IMessenger messenger, object subscriber, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(messenger, nameof(messenger));
            Should.NotBeNull(subscriber, nameof(subscriber));
            var result = false;
            foreach (var c in components)
            {
                if (c.TryUnsubscribe(messenger, subscriber, metadata))
                    result = true;
            }

            return result;
        }

        public static bool TryUnsubscribeAll(this ItemOrArray<IMessengerSubscriberComponent> components, IMessenger messenger, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(messenger, nameof(messenger));
            var result = false;
            foreach (var c in components)
            {
                if (c.TryUnsubscribeAll(messenger, metadata))
                    result = true;
            }

            return result;
        }

        public static bool HasSubscribers(this ItemOrArray<IMessengerSubscriberComponent> components, IMessenger messenger, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(messenger, nameof(messenger));
            foreach (var component in components)
            {
                if (component.HasSubscribers(messenger, metadata))
                    return true;
            }

            return false;
        }

        public static ItemOrIReadOnlyList<MessengerSubscriberInfo> TryGetSubscribers(this ItemOrArray<IMessengerSubscriberComponent> components, IMessenger messenger,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(messenger, nameof(messenger));
            if (components.Count == 0)
                return default;
            if (components.Count == 1)
                return components[0].TryGetSubscribers(messenger, metadata);
            var subscribers = new ItemOrListEditor<MessengerSubscriberInfo>();
            foreach (var c in components)
                subscribers.AddRange(c.TryGetSubscribers(messenger, metadata));

            return subscribers.ToItemOrList();
        }

        public static ItemOrIReadOnlyList<MessengerHandler> TryGetMessengerHandlers(this ItemOrArray<IMessengerSubscriberComponent> components, IMessenger messenger,
            Type messageType, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(messenger, nameof(messenger));
            Should.NotBeNull(messageType, nameof(messageType));
            if (components.Count == 0)
                return default;
            if (components.Count == 1)
                return components[0].TryGetMessengerHandlers(messenger, messageType, metadata);
            var handlers = new ItemOrListEditor<MessengerHandler>();
            foreach (var c in components)
                handlers.AddRange(c.TryGetMessengerHandlers(messenger, messageType, metadata));

            return handlers.ToItemOrList();
        }
    }
}