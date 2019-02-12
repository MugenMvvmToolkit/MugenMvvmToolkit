using System.Threading.Tasks;

namespace MugenMvvm.Interfaces.Messaging
{
    public interface IEventPublisher
    {
        void Publish(object sender, object message, IMessengerContext? messengerContext = null);

        Task PublishAsync(object sender, object message, IMessengerContext? messengerContext = null);
    }
}