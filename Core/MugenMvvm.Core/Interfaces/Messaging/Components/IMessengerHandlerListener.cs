using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Messaging.Components
{
    public interface IMessengerHandlerListener : IComponent<IMessenger>
    {
        void OnHandling(object subscriber, IMessageContext messageContext);

        void OnHandled(MessengerResult? result, object subscriber, IMessageContext messageContext);
    }
}