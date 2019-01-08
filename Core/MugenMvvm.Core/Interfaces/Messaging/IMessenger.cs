using System.Collections.Generic;
using MugenMvvm.Enums;
using MugenMvvm.Infrastructure.Messaging;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Models;

namespace MugenMvvm.Interfaces.Messaging
{
    public interface IMessenger
    {
        IReadOnlyList<MessengerSubscriberInfo> GetSubscribers();

        IMessengerContext GetContext(IMetadataContext? metadata);

        void Publish(object sender, object message, IMessengerContext? messengerContext = null);

        void Subscribe(IMessengerSubscriber subscriber, ThreadExecutionMode? executionMode = null);

        void Unsubscribe(IMessengerSubscriber subscriber);
    }
}