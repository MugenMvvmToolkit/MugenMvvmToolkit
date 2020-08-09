using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Attributes;

namespace MugenMvvm.Interfaces.Messaging
{
    public interface IMessengerHandler
    {
    }

    public interface IMessengerHandler<in TMessage> : IMessengerHandler
    {
        [Preserve(Conditional = true)]
        void Handle([DisallowNull] TMessage message, IMessageContext messageContext);
    }
}