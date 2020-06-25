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

        public readonly Func<object, IMessageContext, object?, MessengerResult> Handler;
        public readonly ThreadExecutionMode? ExecutionMode;
        public readonly object Subscriber;
        public readonly object? State;

        #endregion

        #region Constructors

        public MessengerHandler(Func<object, IMessageContext, object?, MessengerResult> handler, object subscriber, ThreadExecutionMode? executionMode, object? state = null)
        {
            Should.NotBeNull(handler, nameof(handler));
            Should.NotBeNull(subscriber, nameof(subscriber));
            ExecutionMode = executionMode;
            Subscriber = subscriber;
            Handler = handler;
            State = state;
        }

        #endregion

        #region Properties

        public bool IsEmpty => Handler == null;

        #endregion

        #region Methods

        public MessengerResult Handle(IMessageContext messageContext)
        {
            if (Handler == null)
                return MessengerResult.Ignored;
            return Handler(Subscriber, messageContext, State);
        }

        #endregion
    }
}