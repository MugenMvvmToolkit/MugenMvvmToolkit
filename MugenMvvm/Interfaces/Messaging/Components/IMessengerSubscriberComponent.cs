using System;
using System.Collections.Generic;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;
using MugenMvvm.Messaging;

namespace MugenMvvm.Interfaces.Messaging.Components
{
    public interface IMessengerSubscriberComponent : IComponent<IMessenger>//todo add delegate subscriber
    {
        bool TrySubscribe(IMessenger messenger, object subscriber, ThreadExecutionMode? executionMode, IReadOnlyMetadataContext? metadata);

        bool TryUnsubscribe(IMessenger messenger, object subscriber, IReadOnlyMetadataContext? metadata);

        bool TryUnsubscribeAll(IMessenger messenger, IReadOnlyMetadataContext? metadata);

        ItemOrList<MessengerHandler, IReadOnlyList<MessengerHandler>> TryGetMessengerHandlers(IMessenger messenger, Type messageType, IReadOnlyMetadataContext? metadata);

        ItemOrList<MessengerSubscriberInfo, IReadOnlyList<MessengerSubscriberInfo>> TryGetSubscribers(IMessenger messenger, IReadOnlyMetadataContext? metadata);
    }
}