using System;
using MugenMvvm.Enums;

namespace MugenMvvm.Interfaces.Messaging
{
    public interface IMessengerHandlerRaw : IMessengerHandler
    {
        bool CanHandle(Type messageType);

        MessengerResult Handle(IMessageContext messageContext);
    }
}