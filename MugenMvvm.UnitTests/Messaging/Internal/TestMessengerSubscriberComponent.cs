using System;
using System.Collections.Generic;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Messaging.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;
using MugenMvvm.Messaging;
using Should;

namespace MugenMvvm.UnitTests.Messaging.Internal
{
    public class TestMessengerSubscriberComponent : IMessengerSubscriberComponent, IHasPriority
    {
        #region Fields

        private readonly IMessenger? _messenger;

        #endregion

        #region Constructors

        public TestMessengerSubscriberComponent(IMessenger? messenger = null)
        {
            _messenger = messenger;
        }

        #endregion

        #region Properties

        public Func<object?, ThreadExecutionMode?, IReadOnlyMetadataContext?, bool>? TrySubscribe { get; set; }

        public Func<object?, IReadOnlyMetadataContext?, bool>? TryUnsubscribe { get; set; }

        public Func<IReadOnlyMetadataContext?, bool>? TryUnsubscribeAll { get; set; }

        public Func<Type, IReadOnlyMetadataContext?, ItemOrList<MessengerHandler, IReadOnlyList<MessengerHandler>>>? TryGetMessengerHandlers { get; set; }

        public Func<IReadOnlyMetadataContext?, ItemOrList<MessengerSubscriberInfo, IReadOnlyList<MessengerSubscriberInfo>>>? TryGetSubscribers { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

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

        ItemOrList<MessengerHandler, IReadOnlyList<MessengerHandler>> IMessengerSubscriberComponent.TryGetMessengerHandlers(IMessenger messenger, Type messageType, IReadOnlyMetadataContext? metadata)
        {
            _messenger?.ShouldEqual(messenger);
            return TryGetMessengerHandlers?.Invoke(messageType, metadata) ?? default;
        }

        ItemOrList<MessengerSubscriberInfo, IReadOnlyList<MessengerSubscriberInfo>> IMessengerSubscriberComponent.TryGetSubscribers(IMessenger messenger, IReadOnlyMetadataContext? metadata)
        {
            _messenger?.ShouldEqual(messenger);
            return TryGetSubscribers?.Invoke(metadata) ?? default;
        }

        #endregion
    }
}