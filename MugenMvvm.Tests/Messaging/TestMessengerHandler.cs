using System;
using MugenMvvm.Interfaces.Messaging;

namespace MugenMvvm.Tests.Messaging
{
    public class TestMessengerHandler : IMessengerHandler<string>, IMessengerHandler<int>
    {
        public Action<int, IMessageContext>? HandleInt { get; set; }

        public Action<string, IMessageContext>? HandleString { get; set; }

        void IMessengerHandler<int>.Handle(int message, IMessageContext messageContext) => HandleInt?.Invoke(message, messageContext);

        void IMessengerHandler<string>.Handle(string message, IMessageContext messageContext) => HandleString?.Invoke(message, messageContext);
    }
}