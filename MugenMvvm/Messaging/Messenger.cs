﻿using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Messaging.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Messaging
{
    public sealed class Messenger : ComponentOwnerBase<IMessenger>, IMessenger
    {
        public Messenger(IComponentCollectionManager? componentCollectionManager = null)
            : base(componentCollectionManager)
        {
        }

        public IMessageContext GetMessageContext(object? sender, object message, IReadOnlyMetadataContext? metadata = null) =>
            GetComponents<IMessageContextProviderComponent>(metadata).TryGetMessageContext(this, sender, message, metadata) ?? new MessageContext(sender, message, metadata);

        public bool Publish(IMessageContext messageContext) => GetComponents<IMessagePublisherComponent>(messageContext.GetMetadataOrDefault()).TryPublish(this, messageContext);

        public bool TrySubscribe(object subscriber, ThreadExecutionMode? executionMode = null, IReadOnlyMetadataContext? metadata = null)
        {
            var result = GetComponents<IMessengerSubscriberComponent>(metadata).TrySubscribe(this, subscriber, executionMode, metadata);
            if (result)
                this.TryInvalidateCache(subscriber, metadata);
            return result;
        }

        public bool TryUnsubscribe(object subscriber, IReadOnlyMetadataContext? metadata = null)
        {
            var result = GetComponents<IMessengerSubscriberComponent>(metadata).TryUnsubscribe(this, subscriber, metadata);
            if (result)
                this.TryInvalidateCache(subscriber, metadata);
            return result;
        }

        public bool UnsubscribeAll(IReadOnlyMetadataContext? metadata = null)
        {
            var result = GetComponents<IMessengerSubscriberComponent>(metadata).TryUnsubscribeAll(this, metadata);
            if (result)
                this.TryInvalidateCache(metadata);
            return result;
        }

        public bool HasSubscribers(IReadOnlyMetadataContext? metadata = null) => GetComponents<IMessengerSubscriberComponent>().HasSubscribers(this, metadata);

        public ItemOrIReadOnlyList<MessengerSubscriberInfo> GetSubscribers(IReadOnlyMetadataContext? metadata = null) =>
            GetComponents<IMessengerSubscriberComponent>(metadata).TryGetSubscribers(this, metadata);
    }
}