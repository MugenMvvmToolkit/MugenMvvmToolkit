using System;
using System.Collections.Generic;
using MugenMvvm.Enums;
using MugenMvvm.Infrastructure.Messaging;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Messaging
{
    public interface IMessenger : IEventPublisher, IComponentOwner<IMessenger>, IDisposable
    {
        IMessengerContext GetMessengerContext(IMetadataContext? metadata);

        void Subscribe(IMessengerSubscriber subscriber, ThreadExecutionMode executionMode, IReadOnlyMetadataContext metadata);

        bool Unsubscribe(IMessengerSubscriber subscriber, IReadOnlyMetadataContext metadata);

        IReadOnlyList<MessengerSubscriberInfo> GetSubscribers();
    }
}