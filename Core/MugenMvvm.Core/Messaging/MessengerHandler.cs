using System;
using System.Runtime.InteropServices;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Messaging;

namespace MugenMvvm.Messaging
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct MessengerHandler//todo review state
    {
        #region Fields

        public readonly Func<object, object?, IMessageContext, MessengerResult> Handler;
        public readonly ThreadExecutionMode ExecutionMode;
        public readonly object Subscriber;
        public readonly object? State;

        #endregion

        #region Constructors

        public MessengerHandler(Func<object, object?, IMessageContext, MessengerResult> handler, object subscriber, ThreadExecutionMode executionMode, object? state = null)
        {
            Should.NotBeNull(handler, nameof(handler));
            Should.NotBeNull(subscriber, nameof(subscriber));
            Should.NotBeNull(executionMode, nameof(executionMode));
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
            return Handler(Subscriber, State, messageContext);
        }

        #endregion
    }
}