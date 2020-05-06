using System;
using MugenMvvm.Interfaces.Messaging;

namespace MugenMvvm.UnitTest.Messaging.Internal
{
    public class TestMessengerHandlerGeneric<T> : IMessengerHandler<T>
    {
        #region Properties

        public Action<object, Type, IMessageContext>? Handle { get; set; }

        #endregion

        #region Implementation of interfaces

        void IMessengerHandler<T>.Handle(T message, IMessageContext messageContext)
        {
            Handle?.Invoke(message!, typeof(T), messageContext);
        }

        #endregion
    }
}