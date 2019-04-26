using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Messaging
{
    public interface IMessengerListener : IListener
    {
        void OnContextCreated(IMessenger messenger, IMessengerContext messengerContext);

        IMessengerSubscriber OnSubscribing(IMessenger messenger, IMessengerSubscriber subscriber, ThreadExecutionMode executionMode, IReadOnlyMetadataContext metadata);

        void OnSubscribed(IMessenger messenger, IMessengerSubscriber subscriber, ThreadExecutionMode executionMode, IReadOnlyMetadataContext metadata);

        void OnUnsubscribed(IMessenger messenger, IMessengerSubscriber subscriber, IReadOnlyMetadataContext metadata);

        MessengerSubscriberResult? OnPublishing(IMessenger messenger, IMessengerSubscriber subscriber, object sender, object message, IMessengerContext messengerContext);

        void OnPublished(IMessenger messenger, MessengerSubscriberResult result, IMessengerSubscriber subscriber, object sender, object message, IMessengerContext messengerContext);
    }
}