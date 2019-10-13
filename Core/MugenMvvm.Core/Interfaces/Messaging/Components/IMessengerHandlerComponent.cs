using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Messaging.Components
{
    public interface IMessengerHandlerComponent : IComponent<IMessenger>
    {
        bool CanHandle(object subscriber, IMessageContext messageContext);

        MessengerResult? TryHandle(object subscriber, IMessageContext messageContext);
    }
}