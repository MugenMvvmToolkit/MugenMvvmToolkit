﻿using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Messaging;

// ReSharper disable once CheckNamespace
namespace MugenMvvm
{
    public static partial class MugenExtensions
    {
        #region Methods

        public static IMessengerSubscriber SubscribeWeak<TTarget, TMessage>(this IMessenger messenger, TTarget target, Action<TTarget, object, TMessage, IMessengerContext> action,
            ThreadExecutionMode executionMode, IReadOnlyMetadataContext? metadata = null)
            where TTarget : class
        {
            Should.NotBeNull(messenger, nameof(messenger));
            var subscriber = new WeakDelegateMessengerSubscriber<TTarget, TMessage>(target, action);
            messenger.Subscribe(subscriber, executionMode, metadata);
            return subscriber;
        }

        public static IMessengerSubscriber SubscribeWeak<TMessage>(this IMessenger messenger, Action<object, TMessage, IMessengerContext> action,
            ThreadExecutionMode executionMode, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(messenger, nameof(messenger));
            var subscriber = new WeakDelegateMessengerSubscriber<object, TMessage>(action);
            messenger.Subscribe(subscriber, executionMode, metadata);
            return subscriber;
        }

        public static IMessengerSubscriber Subscribe<TMessage>(this IMessenger messenger, Action<object, TMessage, IMessengerContext> action,
            ThreadExecutionMode executionMode, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(messenger, nameof(messenger));
            var subscriber = new DelegateMessengerSubscriber<TMessage>(action);
            messenger.Subscribe(subscriber, executionMode, metadata);
            return subscriber;
        }

        public static IMessengerSubscriber Subscribe(this IMessenger messenger, IMessengerHandler handler,
            ThreadExecutionMode executionMode, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(messenger, nameof(messenger));
            var subscriber = new MessengerHandlerSubscriber(handler);
            messenger.Subscribe(subscriber, executionMode, metadata);
            return subscriber;
        }

        public static void UnsubscribeAll(this IMessenger messenger, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(messenger, nameof(messenger));
            var list = messenger.GetSubscribers();
            for (var index = 0; index < list.Count; index++)
                messenger.Unsubscribe(list[index].Subscriber, metadata);
        }

        #endregion
    }
}