using System.Collections.Generic;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Messaging;

namespace MugenMvvm.Interfaces.Messaging
{
    public interface IMessenger : IMessagePublisher, IComponentOwner<IMessenger>, IComponent<IMugenApplication>
    {
        bool Subscribe<TSubscriber>(in TSubscriber subscriber, ThreadExecutionMode? executionMode = null, IReadOnlyMetadataContext? metadata = null);

        bool Unsubscribe<TSubscriber>(in TSubscriber subscriber, IReadOnlyMetadataContext? metadata = null);

        void UnsubscribeAll(IReadOnlyMetadataContext? metadata = null);

        IReadOnlyList<MessengerSubscriberInfo> GetSubscribers(IReadOnlyMetadataContext? metadata = null);
    }
}