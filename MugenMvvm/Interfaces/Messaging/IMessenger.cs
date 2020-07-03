using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;
using MugenMvvm.Messaging;

namespace MugenMvvm.Interfaces.Messaging
{
    public interface IMessenger : IMessagePublisher, IComponentOwner<IMessenger>
    {
        bool TrySubscribe<TSubscriber>([DisallowNull] in TSubscriber subscriber, ThreadExecutionMode? executionMode = null, IReadOnlyMetadataContext? metadata = null);

        bool TryUnsubscribe<TSubscriber>([DisallowNull] in TSubscriber subscriber, IReadOnlyMetadataContext? metadata = null);

        bool UnsubscribeAll(IReadOnlyMetadataContext? metadata = null);

        ItemOrList<MessengerSubscriberInfo, IReadOnlyList<MessengerSubscriberInfo>> GetSubscribers(IReadOnlyMetadataContext? metadata = null);
    }
}