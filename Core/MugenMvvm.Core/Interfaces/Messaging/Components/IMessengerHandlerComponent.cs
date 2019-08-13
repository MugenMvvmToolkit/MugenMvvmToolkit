using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Messaging.Components
{
    public interface IMessengerHandlerComponent : IComponent<IMessenger>
    {
        MessengerResult? TryHandle(object subscriber, IMessageContext messageContext);
    }
}