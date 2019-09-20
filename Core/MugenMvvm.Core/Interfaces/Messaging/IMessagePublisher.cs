using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Messaging
{
    public interface IMessagePublisher
    {
        IMessageContext Publish(object? sender, object message, IReadOnlyMetadataContext? metadata = null);

        void Publish(IMessageContext messageContext);
    }
}