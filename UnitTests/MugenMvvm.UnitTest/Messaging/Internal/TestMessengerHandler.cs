using System;
using MugenMvvm.Interfaces.Messaging;

namespace MugenMvvm.UnitTest.Messaging.Internal
{
    public class TestMessengerHandler : IMessengerHandler<string>, IMessengerHandler<int>
    {
        #region Properties

        public Action<int, IMessageContext>? HandleInt { get; set; }

        public Action<string, IMessageContext>? HandleString { get; set; }

        #endregion

        #region Implementation of interfaces

        void IMessengerHandler<int>.Handle(int message, IMessageContext messageContext)
        {
            HandleInt?.Invoke(message, messageContext);
        }

        void IMessengerHandler<string>.Handle(string message, IMessageContext messageContext)
        {
            HandleString?.Invoke(message, messageContext);
        }

        #endregion
    }
}