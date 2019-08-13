using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Messaging
{
    public interface IMessageContext : IMetadataOwner<IMetadataContext>
    {
        IMessagePublisher Publisher { get; }

        object? Sender { get; }

        object Message { get; }

        bool MarkAsHandled(object subscriber);
    }
}