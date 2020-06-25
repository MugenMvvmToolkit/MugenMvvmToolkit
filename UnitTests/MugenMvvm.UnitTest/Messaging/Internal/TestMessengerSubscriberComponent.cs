using System;
using System.Collections.Generic;
using MugenMvvm.Enums;
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

        public Func<object?, Type, ThreadExecutionMode?, IReadOnlyMetadataContext?, bool>? TrySubscribe { get; set; }

        public Func<object?, Type, IReadOnlyMetadataContext?, bool>? TryUnsubscribe { get; set; }

        public Action<IReadOnlyMetadataContext?>? TryUnsubscribeAll { get; set; }

        public Func<Type, IReadOnlyMetadataContext?, ItemOrList<MessengerHandler, IReadOnlyList<MessengerHandler>>>? TryGetMessengerHandlers { get; set; }

        public Func<IReadOnlyMetadataContext?, ItemOrList<MessengerSubscriberInfo, IReadOnlyList<MessengerSubscriberInfo>>>? TryGetSubscribers { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        bool IMessengerSubscriberComponent.TrySubscribe<TSubscriber>(in TSubscriber subscriber, ThreadExecutionMode? executionMode, IReadOnlyMetadataContext? metadata)
        {
            return TrySubscribe?.Invoke(subscriber, typeof(TSubscriber), executionMode, metadata) ?? false;
        }

        bool IMessengerSubscriberComponent.TryUnsubscribe<TSubscriber>(in TSubscriber subscriber, IReadOnlyMetadataContext? metadata)
        {
            return TryUnsubscribe?.Invoke(subscriber, typeof(TSubscriber), metadata) ?? false;
        }

        void IMessengerSubscriberComponent.TryUnsubscribeAll(IReadOnlyMetadataContext? metadata)
        {
            TryUnsubscribeAll?.Invoke(metadata);
        }

        ItemOrList<MessengerHandler, IReadOnlyList<MessengerHandler>> IMessengerSubscriberComponent.TryGetMessengerHandlers(Type messageType, IReadOnlyMetadataContext? metadata)
        {
            return TryGetMessengerHandlers?.Invoke(messageType, metadata) ?? default;
        }

        ItemOrList<MessengerSubscriberInfo, IReadOnlyList<MessengerSubscriberInfo>> IMessengerSubscriberComponent.TryGetSubscribers(IReadOnlyMetadataContext? metadata)
        {
            return TryGetSubscribers?.Invoke(metadata) ?? default;
        }

        #endregion
    }
}