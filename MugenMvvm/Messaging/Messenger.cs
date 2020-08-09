using System.Collections.Generic;
using MugenMvvm.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Messaging.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Messaging
{
    public sealed class Messenger : ComponentOwnerBase<IMessenger>, IMessenger
    {
        #region Constructors

        public Messenger(IComponentCollectionManager? componentCollectionManager = null)
            : base(componentCollectionManager)
        {
        }

        #endregion

        #region Implementation of interfaces

        public IMessageContext GetMessageContext(object? sender, object message, IReadOnlyMetadataContext? metadata = null) =>
            GetComponents<IMessageContextProviderComponent>().TryGetMessageContext(this, sender, message, metadata) ?? new MessageContext(sender, message, metadata);

        public bool Publish(IMessageContext messageContext) => GetComponents<IMessagePublisherComponent>().TryPublish(this, messageContext);

        public bool TrySubscribe(object subscriber, ThreadExecutionMode? executionMode = null, IReadOnlyMetadataContext? metadata = null)
        {
            var result = GetComponents<IMessengerSubscriberComponent>().TrySubscribe(this, subscriber, executionMode, metadata);
            if (result)
                this.TryInvalidateCache(subscriber, metadata);
            return result;
        }

        public bool TryUnsubscribe(object subscriber, IReadOnlyMetadataContext? metadata = null)
        {
            var result = GetComponents<IMessengerSubscriberComponent>().TryUnsubscribe(this, subscriber, metadata);
            if (result)
                this.TryInvalidateCache(subscriber, metadata);
            return result;
        }

        public bool UnsubscribeAll(IReadOnlyMetadataContext? metadata = null)
        {
            var result = GetComponents<IMessengerSubscriberComponent>().TryUnsubscribeAll(this, metadata);
            if (result)
                this.TryInvalidateCache(metadata);
            return result;
        }

        public ItemOrList<MessengerSubscriberInfo, IReadOnlyList<MessengerSubscriberInfo>> GetSubscribers(IReadOnlyMetadataContext? metadata = null) =>
            GetComponents<IMessengerSubscriberComponent>().TryGetSubscribers(this, metadata);

        #endregion
    }
}