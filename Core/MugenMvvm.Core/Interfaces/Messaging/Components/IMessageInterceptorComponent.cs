using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Messaging.Components
{
    public interface IMessageInterceptorComponent : IComponent<IMessenger>
    {
        MessengerSubscriberResult? OnPublishing(IMessengerSubscriber subscriber, object sender, object message, IMessengerContext messengerContext);

        void OnPublished(MessengerSubscriberResult result, IMessengerSubscriber subscriber, object sender, object message, IMessengerContext messengerContext);
    }
}