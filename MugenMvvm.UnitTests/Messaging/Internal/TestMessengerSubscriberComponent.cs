﻿using System;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Messaging.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Messaging;
using Should;

namespace MugenMvvm.UnitTests.Messaging.Internal
{
    public class TestMessengerSubscriberComponent : IMessengerSubscriberComponent, IHasPriority
    {
        private readonly IMessenger? _messenger;

        public TestMessengerSubscriberComponent(IMessenger? messenger = null)
        {
            _messenger = messenger;
        }

        public Func<object?, ThreadExecutionMode?, IReadOnlyMetadataContext?, bool>? TrySubscribe { get; set; }

        public Func<object?, IReadOnlyMetadataContext?, bool>? TryUnsubscribe { get; set; }

        public Func<IReadOnlyMetadataContext?, bool>? TryUnsubscribeAll { get; set; }

        public Func<Type, IReadOnlyMetadataContext?, ItemOrIReadOnlyList<MessengerHandler>>? TryGetMessengerHandlers { get; set; }

        public Func<IReadOnlyMetadataContext?, ItemOrIReadOnlyList<MessengerSubscriberInfo>>? TryGetSubscribers { get; set; }

        public int Priority { get; set; }

        bool IMessengerSubscriberComponent.TrySubscribe(IMessenger messenger, object subscriber, ThreadExecutionMode? executionMode, IReadOnlyMetadataContext? metadata)
        {
            _messenger?.ShouldEqual(messenger);
            return TrySubscribe?.Invoke(subscriber, executionMode, metadata) ?? false;
        }

        bool IMessengerSubscriberComponent.TryUnsubscribe(IMessenger messenger, object subscriber, IReadOnlyMetadataContext? metadata)
        {
            _messenger?.ShouldEqual(messenger);
            return TryUnsubscribe?.Invoke(subscriber, metadata) ?? false;
        }

        bool IMessengerSubscriberComponent.TryUnsubscribeAll(IMessenger messenger, IReadOnlyMetadataContext? metadata)
        {
            _messenger?.ShouldEqual(messenger);
            return TryUnsubscribeAll?.Invoke(metadata) ?? false;
        }

        ItemOrIReadOnlyList<MessengerHandler> IMessengerSubscriberComponent.TryGetMessengerHandlers(IMessenger messenger, Type messageType, IReadOnlyMetadataContext? metadata)
        {
            _messenger?.ShouldEqual(messenger);
            return TryGetMessengerHandlers?.Invoke(messageType, metadata) ?? default;
        }

        ItemOrIReadOnlyList<MessengerSubscriberInfo> IMessengerSubscriberComponent.TryGetSubscribers(IMessenger messenger, IReadOnlyMetadataContext? metadata)
        {
            _messenger?.ShouldEqual(messenger);
            return TryGetSubscribers?.Invoke(metadata) ?? default;
        }
    }
}