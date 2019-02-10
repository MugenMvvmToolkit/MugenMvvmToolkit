namespace MugenMvvm.Interfaces.Messaging
{
    public interface IEventPublisher
    {
        void Publish(object sender, object message, IMessengerContext? messengerContext = null);
    }
}