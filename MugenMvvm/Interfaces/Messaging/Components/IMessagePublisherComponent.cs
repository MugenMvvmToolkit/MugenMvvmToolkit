using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Messaging.Components
{
    public interface IMessagePublisherComponent : IComponent<IMessenger>
    {
        bool TryPublish(IMessenger messenger, IMessageContext messageContext);
    }
}