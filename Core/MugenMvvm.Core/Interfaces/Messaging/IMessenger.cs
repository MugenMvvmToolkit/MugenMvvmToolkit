using System;
using System.Collections.Generic;
using MugenMvvm.Enums;
using MugenMvvm.Infrastructure.Messaging;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Messaging
{
    public interface IMessenger : IEventPublisher, IHasListeners<IMessengerListener>, IDisposable
    {
        IMessengerContext GetMessengerContext(IMetadataContext? metadata);

        void Subscribe(IMessengerSubscriber subscriber, ThreadExecutionMode executionMode);//todo add metadata

        bool Unsubscribe(IMessengerSubscriber subscriber);

        IReadOnlyList<MessengerSubscriberInfo> GetSubscribers();
    }
}