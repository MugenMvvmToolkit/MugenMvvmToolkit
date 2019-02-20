using System.Collections.Generic;
using MugenMvvm.Enums;
using MugenMvvm.Infrastructure.Messaging;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Messaging
{
    public interface IMessenger : IEventPublisher//todo onsubscribing? ,extend viewmodel presenter listener onpresenter added/adding, check all listeners
    {
        IReadOnlyList<MessengerSubscriberInfo> GetSubscribers();

        IMessengerContext GetContext(IMetadataContext? metadata);

        void Subscribe(IMessengerSubscriber subscriber, ThreadExecutionMode executionMode);//todo execution mode?

        bool Unsubscribe(IMessengerSubscriber subscriber);
    }
}