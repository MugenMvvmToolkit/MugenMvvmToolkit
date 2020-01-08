using System;
using System.Collections.Generic;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Messaging.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Messaging;

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

        public static void TryPublish(this IMessagePublisherComponent[] components, IMessageContext messageContext)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(messageContext, nameof(messageContext));
            for (var i = 0; i < components.Length; i++)
                components[i].TryPublish(messageContext);
        }

        public static bool TrySubscribe<TSubscriber>(this IMessengerSubscriberComponent[] components, in TSubscriber subscriber, ThreadExecutionMode? executionMode, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            var result = false;
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].TrySubscribe(subscriber, executionMode, metadata))
                    result = true;
            }

            return result;
        }

        public static bool TryUnsubscribe<TSubscriber>(this IMessengerSubscriberComponent[] components, in TSubscriber subscriber, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            var result = false;
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].TryUnsubscribe(subscriber, metadata))
                    result = true;
            }

            return result;
        }

        public static void TryUnsubscribeAll(this IMessengerSubscriberComponent[] components, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
                components[i].TryUnsubscribeAll(metadata);
        }

        public static IReadOnlyList<MessengerSubscriberInfo>? TryGetSubscribers(this IMessengerSubscriberComponent[] components, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            List<MessengerSubscriberInfo>? subscribers = null; //todo ext method
            for (var i = 0; i < components.Length; i++)
            {
                var list = components[i].TryGetSubscribers(metadata);
                if (list == null || list.Count == 0)
                    continue;
                if (subscribers == null)
                    subscribers = new List<MessengerSubscriberInfo>();
                subscribers.AddRange(list);
            }

            return subscribers;
        }

        public static IReadOnlyList<(ThreadExecutionMode, MessengerHandler)>? TryGetMessengerHandlers(this IMessengerSubscriberComponent[] components, Type messageType, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(messageType, nameof(messageType));
            List<(ThreadExecutionMode, MessengerHandler)>? handlers = null;
            for (var i = 0; i < components.Length; i++)
                MugenExtensions.AddIfNotNullOrEmpty(ref handlers, components[i].TryGetMessengerHandlers(messageType, metadata));
            return handlers;
        }

        #endregion
    }
}