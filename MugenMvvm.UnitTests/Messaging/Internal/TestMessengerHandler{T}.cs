using System;
using MugenMvvm.Interfaces.Messaging;

namespace MugenMvvm.UnitTests.Messaging.Internal
{
    public class TestMessengerHandlerGeneric<T> : IMessengerHandler<T>
    {
        public Action<object, Type, IMessageContext>? Handle { get; set; }

        void IMessengerHandler<T>.Handle(T message, IMessageContext messageContext) => Handle?.Invoke(message!, typeof(T), messageContext);
    }
}