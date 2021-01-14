using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Metadata;

namespace MugenMvvm.Messaging
{
    public sealed class MessageContext : MetadataOwnerBase, IMessageContext
    {
        public MessageContext(object? sender, object message, IReadOnlyMetadataContext? metadata = null)
            : base(metadata)
        {
            Should.NotBeNull(message, nameof(message));
            Sender = sender;
            Message = message;
        }

        public object? Sender { get; }

        public object Message { get; }
    }
}