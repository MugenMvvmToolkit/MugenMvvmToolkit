using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Messaging
{
    public interface IMessengerListener : IListener
    {
        void OnContextCreated(IMessenger messenger, IMessengerContext messengerContext);


        IMessengerSubscriber OnSubscribing(IMessenger messenger, IMessengerSubscriber subscriber, ThreadExecutionMode executionMode);

        void OnSubscribed(IMessenger messenger, IMessengerSubscriber subscriber, ThreadExecutionMode executionMode);

        void OnUnsubscribed(IMessenger messenger, IMessengerSubscriber subscriber);


        MessengerSubscriberResult? OnPublishing(IMessenger messenger, IMessengerSubscriber subscriber, object sender, object message, IMessengerContext messengerContext);

        void OnPublished(IMessenger messenger, IMessengerSubscriber subscriber, object sender, object message, IMessengerContext messengerContext, MessengerSubscriberResult result);
    }
}