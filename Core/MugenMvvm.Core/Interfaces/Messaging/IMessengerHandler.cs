using MugenMvvm.Attributes;

namespace MugenMvvm.Interfaces.Messaging
{
    public interface IMessengerHandler { }

    public interface IMessengerHandler<in TMessage> : IMessengerHandler
    {
        [Preserve(Conditional = true)]
        void Handle(object sender, TMessage message, IMessengerContext messengerContext);
    }
}