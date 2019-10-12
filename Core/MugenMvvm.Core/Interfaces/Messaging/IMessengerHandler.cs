using MugenMvvm.Attributes;

namespace MugenMvvm.Interfaces.Messaging
{
    public interface IMessengerHandler
    {
    }

    public interface IMessengerHandler<TMessage> : IMessengerHandler//todo update handler
    {
        [Preserve(Conditional = true)]
        void Handle(TMessage message, IMessageContext messageContext);
    }
}