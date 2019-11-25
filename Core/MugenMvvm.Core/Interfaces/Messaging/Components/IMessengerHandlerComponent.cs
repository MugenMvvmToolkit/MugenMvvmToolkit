using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Messaging.Components
{
    public interface IMessengerHandlerComponent : IComponent<IMessenger>
    {
        bool CanHandle(object subscriber, Type messageType);

        MessengerResult? TryHandle(object subscriber, IMessageContext messageContext);
    }
}