using System;
using System.Collections.Generic;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Messaging.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;
using MugenMvvm.Messaging;

namespace MugenMvvm.UnitTest.Messaging.Internal
{
    public class TestMessengerSubscriberComponent : IMessengerSubscriberComponent, IHasPriority
    {
        #region Properties

        public Func<IMessenger, object?, Type, ThreadExecutionMode?, IReadOnlyMetadataContext?, bool>? TrySubscribe { get; set; }

        public Func<IMessenger, object?, Type, IReadOnlyMetadataContext?, bool>? TryUnsubscribe { get; set; }

        public Func<IMessenger, IReadOnlyMetadataContext?, bool>? TryUnsubscribeAll { get; set; }

        public Func<IMessenger, Type, IReadOnlyMetadataContext?, ItemOrList<MessengerHandler, IReadOnlyList<MessengerHandler>>>? TryGetMessengerHandlers { get; set; }

        public Func<IMessenger, IReadOnlyMetadataContext?, ItemOrList<MessengerSubscriberInfo, IReadOnlyList<MessengerSubscriberInfo>>>? TryGetSubscribers { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        bool IMessengerSubscriberComponent.TrySubscribe<TSubscriber>(IMessenger messenger, in TSubscriber subscriber, ThreadExecutionMode? executionMode, IReadOnlyMetadataContext? metadata)
        {
            return TrySubscribe?.Invoke(messenger, subscriber, typeof(TSubscriber), executionMode, metadata) ?? false;
        }

        bool IMessengerSubscriberComponent.TryUnsubscribe<TSubscriber>(IMessenger messenger, in TSubscriber subscriber, IReadOnlyMetadataContext? metadata)
        {
            return TryUnsubscribe?.Invoke(messenger, subscriber, typeof(TSubscriber), metadata) ?? false;
        }

        bool IMessengerSubscriberComponent.TryUnsubscribeAll(IMessenger messenger, IReadOnlyMetadataContext? metadata)
        {
            return TryUnsubscribeAll?.Invoke(messenger, metadata) ?? false;
        }

        ItemOrList<MessengerHandler, IReadOnlyList<MessengerHandler>> IMessengerSubscriberComponent.TryGetMessengerHandlers(IMessenger messenger, Type messageType, IReadOnlyMetadataContext? metadata)
        {
            return TryGetMessengerHandlers?.Invoke(messenger, messageType, metadata) ?? default;
        }

        ItemOrList<MessengerSubscriberInfo, IReadOnlyList<MessengerSubscriberInfo>> IMessengerSubscriberComponent.TryGetSubscribers(IMessenger messenger, IReadOnlyMetadataContext? metadata)
        {
            return TryGetSubscribers?.Invoke(messenger, metadata) ?? default;
        }

        #endregion
    }
}