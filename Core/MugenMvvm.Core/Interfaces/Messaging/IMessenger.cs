using System;
using System.Collections.Generic;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Messaging;

namespace MugenMvvm.Interfaces.Messaging
{
    public interface IMessenger : IEventPublisher, IComponentOwner<IMessenger>, IDisposable//todo remove IMessengerSubscriber, common interface, wait execution?
    {
        IMessengerContext GetMessengerContext(IMetadataContext? metadata = null);

        void Subscribe(IMessengerSubscriber subscriber, ThreadExecutionMode executionMode, IReadOnlyMetadataContext? metadata = null);

        bool Unsubscribe(IMessengerSubscriber subscriber, IReadOnlyMetadataContext? metadata = null);

        IReadOnlyList<MessengerSubscriberInfo> GetSubscribers();
    }
}