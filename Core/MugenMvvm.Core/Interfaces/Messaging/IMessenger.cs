using System.Collections.Generic;
using MugenMvvm.Enums;
using MugenMvvm.Infrastructure.Messaging;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Messaging
{
    public interface IMessenger : IEventPublisher, IHasListeners<IMessengerListener> //todo onsubscribing? ,extend viewmodel presenter listener onpresenter added/adding, check all listeners
    {
        IMessengerContext GetContext(IMetadataContext? metadata);

        void Subscribe(IMessengerSubscriber subscriber, ThreadExecutionMode executionMode); //todo execution mode?

        bool Unsubscribe(IMessengerSubscriber subscriber);

        IReadOnlyList<MessengerSubscriberInfo> GetSubscribers();
    }
}