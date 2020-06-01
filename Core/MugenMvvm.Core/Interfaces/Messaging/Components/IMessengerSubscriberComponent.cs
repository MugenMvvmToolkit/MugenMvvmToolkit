using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;
using MugenMvvm.Messaging;

namespace MugenMvvm.Interfaces.Messaging.Components
{
    public interface IMessengerSubscriberComponent : IComponent<IMessenger>
    {
        bool TrySubscribe<TSubscriber>([DisallowNull] in TSubscriber subscriber, ThreadExecutionMode? executionMode, IReadOnlyMetadataContext? metadata);

        bool TryUnsubscribe<TSubscriber>([DisallowNull] in TSubscriber subscriber, IReadOnlyMetadataContext? metadata);

        void TryUnsubscribeAll(IReadOnlyMetadataContext? metadata);

        ItemOrList<MessengerHandler, IReadOnlyList<MessengerHandler>> TryGetMessengerHandlers(Type messageType, IReadOnlyMetadataContext? metadata);

        ItemOrList<MessengerSubscriberInfo, IReadOnlyList<MessengerSubscriberInfo>> TryGetSubscribers(IReadOnlyMetadataContext? metadata);
    }
}