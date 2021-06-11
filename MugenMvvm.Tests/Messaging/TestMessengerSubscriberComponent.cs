using System;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Messaging.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Messaging;

namespace MugenMvvm.Tests.Messaging
{
    public class TestMessengerSubscriberComponent : IMessengerSubscriberComponent, IHasPriority
    {
        public Func<IMessenger, object?, ThreadExecutionMode?, IReadOnlyMetadataContext?, bool>? TrySubscribe { get; set; }

        public Func<IMessenger, object?, IReadOnlyMetadataContext?, bool>? TryUnsubscribe { get; set; }

        public Func<IMessenger, IReadOnlyMetadataContext?, bool>? TryUnsubscribeAll { get; set; }

        public Func<IMessenger, IReadOnlyMetadataContext?, bool>? HasSubscribers { get; set; }

        public Func<IMessenger, Type, IReadOnlyMetadataContext?, ItemOrIReadOnlyList<MessengerHandler>>? TryGetMessengerHandlers { get; set; }

        public Func<IMessenger, IReadOnlyMetadataContext?, ItemOrIReadOnlyList<MessengerSubscriberInfo>>? TryGetSubscribers { get; set; }

        public int Priority { get; set; }

        bool IMessengerSubscriberComponent.TrySubscribe(IMessenger messenger, object subscriber, ThreadExecutionMode? executionMode, IReadOnlyMetadataContext? metadata) =>
            TrySubscribe?.Invoke(messenger, subscriber, executionMode, metadata) ?? false;

        bool IMessengerSubscriberComponent.TryUnsubscribe(IMessenger messenger, object subscriber, IReadOnlyMetadataContext? metadata) =>
            TryUnsubscribe?.Invoke(messenger, subscriber, metadata) ?? false;

        bool IMessengerSubscriberComponent.TryUnsubscribeAll(IMessenger messenger, IReadOnlyMetadataContext? metadata) => TryUnsubscribeAll?.Invoke(messenger, metadata) ?? false;

        bool IMessengerSubscriberComponent.HasSubscribers(IMessenger messenger, IReadOnlyMetadataContext? metadata) => HasSubscribers?.Invoke(messenger, metadata) ?? false;

        ItemOrIReadOnlyList<MessengerHandler> IMessengerSubscriberComponent.TryGetMessengerHandlers(IMessenger messenger, Type messageType, IReadOnlyMetadataContext? metadata) =>
            TryGetMessengerHandlers?.Invoke(messenger, messageType, metadata) ?? default;

        ItemOrIReadOnlyList<MessengerSubscriberInfo> IMessengerSubscriberComponent.TryGetSubscribers(IMessenger messenger, IReadOnlyMetadataContext? metadata) =>
            TryGetSubscribers?.Invoke(messenger, metadata) ?? default;
    }
}