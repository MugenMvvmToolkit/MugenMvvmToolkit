using MugenMvvm.Enums;

namespace MugenMvvm.Interfaces.Messaging
{
    public interface IMessengerListener
    {
        void OnContextCreated(IMessenger messenger, IMessengerContext messengerContext);

        void OnSubscribed(IMessenger messenger, IMessengerSubscriber subscriber, ThreadExecutionMode executionMode);

        void OnUnsubscribed(IMessenger messenger, IMessengerSubscriber subscriber);

        MessengerSubscriberResult? OnPublishing(IMessenger messenger, IMessengerSubscriber subscriber, object sender, object message, IMessengerContext messengerContext);

        void OnPublished(IMessenger messenger, IMessengerSubscriber subscriber, object sender, object message, IMessengerContext messengerContext, MessengerSubscriberResult result);
    }
}