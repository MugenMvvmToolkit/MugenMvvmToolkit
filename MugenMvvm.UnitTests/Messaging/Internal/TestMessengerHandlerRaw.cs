using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Messaging;

namespace MugenMvvm.UnitTests.Messaging.Internal
{
    public class TestMessengerHandlerRaw : IMessengerHandler
    {
        public Func<Type, bool>? CanHandle { get; set; }

        public Func<IMessageContext, MessengerResult>? Handle { get; set; }

        bool IMessengerHandler.CanHandle(Type messageType) => CanHandle?.Invoke(messageType) ?? false;

        MessengerResult IMessengerHandler.Handle(IMessageContext messageContext) => Handle?.Invoke(messageContext) ?? MessengerResult.Ignored;
    }
}