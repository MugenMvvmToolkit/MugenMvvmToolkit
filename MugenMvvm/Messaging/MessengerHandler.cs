using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Messaging;

namespace MugenMvvm.Messaging
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct MessengerHandler
    {
        public readonly object? Subscriber;
        public readonly ThreadExecutionMode? ExecutionMode;
        private readonly Func<object, IMessageContext, object?, MessengerResult>? _handler;
        private readonly object? _state;

        public MessengerHandler(Func<object, IMessageContext, object?, MessengerResult> handler, object subscriber, ThreadExecutionMode? executionMode, object? state = null)
        {
            Should.NotBeNull(handler, nameof(handler));
            Should.NotBeNull(subscriber, nameof(subscriber));
            ExecutionMode = executionMode;
            Subscriber = subscriber;
            _handler = handler;
            _state = state;
        }

        [MemberNotNullWhen(false, nameof(_handler))]
        public bool IsEmpty => _handler == null;

        public void Deconstruct(out object? subscriber, out ThreadExecutionMode? executionMode, out Func<object, IMessageContext, object?, MessengerResult>? handler,
            out object? state)
        {
            subscriber = Subscriber;
            executionMode = ExecutionMode;
            handler = _handler;
            state = _state;
        }

        public MessengerResult Handle(IMessageContext messageContext)
        {
            if (_handler == null)
                return MessengerResult.Ignored;
            return _handler(Subscriber!, messageContext, _state);
        }
    }
}