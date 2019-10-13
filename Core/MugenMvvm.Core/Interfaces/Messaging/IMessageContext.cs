using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Messaging
{
    public interface IMessageContext : IMetadataOwner<IMetadataContext>
    {
        object? Sender { get; }

        object Message { get; }
    }
}