using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Messaging;

namespace MugenMvvm.UnitTests.Messaging.Internal
{
    public class TestMessengerHandlerRaw : IMessengerHandlerRaw
    {
        #region Properties

        public Func<Type, bool>? CanHandle { get; set; }

        public Func<IMessageContext, MessengerResult>? Handle { get; set; }

        #endregion

        #region Implementation of interfaces

        bool IMessengerHandlerRaw.CanHandle(Type messageType) => CanHandle?.Invoke(messageType) ?? false;

        MessengerResult IMessengerHandlerRaw.Handle(IMessageContext messageContext) => Handle?.Invoke(messageContext) ?? MessengerResult.Ignored;

        #endregion
    }
}