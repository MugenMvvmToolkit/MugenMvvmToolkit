using System;
using System.Runtime.InteropServices;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Messaging;

namespace MugenMvvm.Messaging
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct MessengerHandler
    {
        #region Fields

        public readonly Func<object, object?, IMessageContext, MessengerResult> Handler;
        public readonly object Subscriber;
        public readonly object? State;

        #endregion

        #region Constructors

        public MessengerHandler(Func<object, object?, IMessageContext, MessengerResult> handler, object subscriber, object? state = null)
        {
            Should.NotBeNull(handler, nameof(handler));
            Should.NotBeNull(subscriber, nameof(subscriber));
            Subscriber = subscriber;
            Handler = handler;
            State = state;
        }

        #endregion

        #region Properties

        public bool IsEmpty => Subscriber == null;

        #endregion

        #region Methods

        public MessengerResult Handle(IMessageContext messageContext)
        {
            if (Handler == null)
                return MessengerResult.Ignored;
            return Handler(Subscriber, State, messageContext);
        }

        #endregion
    }
}