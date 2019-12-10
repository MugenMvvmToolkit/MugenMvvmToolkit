using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Messaging.Components
{
    public interface IMessengerHandlerListener : IComponent<IMessenger>
    {
        void OnHandling(IMessenger messenger, object subscriber, IMessageContext messageContext);

        void OnHandled(IMessenger messenger, MessengerResult? result, object subscriber, IMessageContext messageContext);
    }
}