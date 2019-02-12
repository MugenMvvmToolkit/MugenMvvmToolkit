using System.Collections.Generic;
using MugenMvvm.Enums;
using MugenMvvm.Infrastructure.Messaging;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Messaging
{
    public interface IMessenger : IEventPublisher
    {
        IReadOnlyList<MessengerSubscriberInfo> GetSubscribers();

        IMessengerContext GetContext(IMetadataContext? metadata);

        void Subscribe(IMessengerSubscriber subscriber, ThreadExecutionMode? executionMode = null);

        bool Unsubscribe(IMessengerSubscriber subscriber);
    }
}