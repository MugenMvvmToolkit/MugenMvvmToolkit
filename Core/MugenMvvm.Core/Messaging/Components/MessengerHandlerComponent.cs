using System;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Messaging.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Messaging.Components
{
    public sealed class MessengerHandlerComponent : IMessengerHandlerComponent, IMessengerSubscriberComponent, IHasPriority
    {
        #region Fields

        public static readonly MessengerHandlerComponent Instance = new MessengerHandlerComponent();

        #endregion

        #region Properties

        public int Priority { get; set; } = MessengerComponentPriority.Handler;

        #endregion

        #region Implementation of interfaces

        public bool CanHandle(object subscriber, Type messageType)
        {
            if (subscriber is IMessengerSubscriber messengerSubscriber)
                return messengerSubscriber.CanHandle(messageType);
            return false;
        }

        public MessengerResult? TryHandle(object subscriber, IMessageContext messageContext)
        {
            if (subscriber is IMessengerSubscriber messengerSubscriber)
                return messengerSubscriber.Handle(messageContext);
            return null;
        }

        public object? TryGetSubscriber(object subscriber, ThreadExecutionMode executionMode, IReadOnlyMetadataContext? metadata)
        {
            return subscriber as IMessengerSubscriber;
        }

        #endregion

        #region Nested types

        public interface IMessengerSubscriber
        {
            bool CanHandle(Type messageType);

            MessengerResult Handle(IMessageContext messageContext);
        }

        #endregion
    }
}