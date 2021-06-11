using System;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Messaging;

namespace MugenMvvm.Interfaces.Messaging.Components
{
    public interface IMessengerSubscriberComponent : IComponent<IMessenger>
    {
        bool TrySubscribe(IMessenger messenger, object subscriber, ThreadExecutionMode? executionMode, IReadOnlyMetadataContext? metadata);

        bool TryUnsubscribe(IMessenger messenger, object subscriber, IReadOnlyMetadataContext? metadata);

        bool TryUnsubscribeAll(IMessenger messenger, IReadOnlyMetadataContext? metadata);

        bool HasSubscribers(IMessenger messenger, IReadOnlyMetadataContext? metadata);

        ItemOrIReadOnlyList<MessengerHandler> TryGetMessengerHandlers(IMessenger messenger, Type messageType, IReadOnlyMetadataContext? metadata);

        ItemOrIReadOnlyList<MessengerSubscriberInfo> TryGetSubscribers(IMessenger messenger, IReadOnlyMetadataContext? metadata);
    }
}