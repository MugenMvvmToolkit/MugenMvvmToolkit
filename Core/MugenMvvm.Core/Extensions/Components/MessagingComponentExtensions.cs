using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

        public static bool TrySubscribe<TSubscriber>(this IMessengerSubscriberComponent[] components, [DisallowNull] in TSubscriber subscriber, ThreadExecutionMode? executionMode, IReadOnlyMetadataContext? metadata)
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

        public static bool TryUnsubscribe<TSubscriber>(this IMessengerSubscriberComponent[] components, [DisallowNull] in TSubscriber subscriber, IReadOnlyMetadataContext? metadata)
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

        public static ItemOrList<MessengerSubscriberInfo, IReadOnlyList<MessengerSubscriberInfo>> TryGetSubscribers(this IMessengerSubscriberComponent[] components, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            if (components.Length == 1)
                return components[0].TryGetSubscribers(metadata);
            ItemOrList<MessengerSubscriberInfo, List<MessengerSubscriberInfo>> subscribers = default;
            for (var i = 0; i < components.Length; i++)
                subscribers.AddRange(components[i].TryGetSubscribers(metadata), info => info.IsEmpty);
            return subscribers.Cast<IReadOnlyList<MessengerSubscriberInfo>>();
        }

        public static ItemOrList<MessengerHandler, IReadOnlyList<MessengerHandler>> TryGetMessengerHandlers(this IMessengerSubscriberComponent[] components, Type messageType, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(messageType, nameof(messageType));
            if (components.Length == 1)
                return components[0].TryGetMessengerHandlers(messageType, metadata);
            ItemOrList<MessengerHandler, List<MessengerHandler>> handlers = default;
            for (var i = 0; i < components.Length; i++)
                handlers.AddRange(components[i].TryGetMessengerHandlers(messageType, metadata), handler => handler.IsEmpty);
            return handlers.Cast<IReadOnlyList<MessengerHandler>>();
        }

        #endregion
    }
}