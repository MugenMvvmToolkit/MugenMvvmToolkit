using System;
using System.Collections.Generic;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Messaging;

namespace MugenMvvm.Interfaces.Messaging
{
    public interface IMessenger : IMessagePublisher, IComponentOwner<IMessenger>, IComponent<IMugenApplication>, IDisposable
    {
        bool Subscribe(object subscriber, ThreadExecutionMode? executionMode = null, IReadOnlyMetadataContext? metadata = null);

        bool Unsubscribe(object subscriber, IReadOnlyMetadataContext? metadata = null);

        IReadOnlyList<MessengerSubscriberInfo> GetSubscribers();
    }
}