using System;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Attributes;
using MugenMvvm.Enums;

namespace MugenMvvm.Interfaces.Messaging
{
    public interface IMessengerHandler : IMessengerHandlerBase
    {
        bool CanHandle(Type messageType);

        MessengerResult Handle(IMessageContext messageContext);
    }

    public interface IMessengerHandler<in TMessage> : IMessengerHandlerBase
    {
        [Preserve(Conditional = true)]
        void Handle([DisallowNull] TMessage message, IMessageContext messageContext);
    }
}