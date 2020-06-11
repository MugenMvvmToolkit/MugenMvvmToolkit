using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Components;
using MugenMvvm.Enums;
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
        #region Fields

        private readonly IMetadataContextProvider? _metadataContextProvider;

        #endregion

        #region Constructors

        public Messenger(IComponentCollectionProvider? componentCollectionProvider = null, IMetadataContextProvider? metadataContextProvider = null)
            : base(componentCollectionProvider)
        {
            _metadataContextProvider = metadataContextProvider;
        }

        #endregion

        #region Implementation of interfaces

        public IMessageContext GetMessageContext(object? sender, object message, IReadOnlyMetadataContext? metadata = null)
        {
            return GetComponents<IMessageContextProviderComponent>().TryGetMessageContext(sender, message, metadata) ?? new MessageContext(sender, message, metadata, _metadataContextProvider);
        }

        public bool Publish(IMessageContext messageContext)
        {
            return GetComponents<IMessagePublisherComponent>().TryPublish(messageContext);
        }

        public bool Subscribe<TSubscriber>([DisallowNull]in TSubscriber subscriber, ThreadExecutionMode? executionMode = null, IReadOnlyMetadataContext? metadata = null)
        {
            return GetComponents<IMessengerSubscriberComponent>().TrySubscribe(subscriber, executionMode, metadata);
        }

        public bool Unsubscribe<TSubscriber>([DisallowNull]in TSubscriber subscriber, IReadOnlyMetadataContext? metadata = null)
        {
            return GetComponents<IMessengerSubscriberComponent>().TryUnsubscribe(subscriber, metadata);
        }

        public void UnsubscribeAll(IReadOnlyMetadataContext? metadata = null)
        {
            GetComponents<IMessengerSubscriberComponent>().TryUnsubscribeAll(metadata);
        }

        public ItemOrList<MessengerSubscriberInfo, IReadOnlyList<MessengerSubscriberInfo>> GetSubscribers(IReadOnlyMetadataContext? metadata = null)
        {
            return GetComponents<IMessengerSubscriberComponent>().TryGetSubscribers(metadata);
        }

        #endregion
    }
}