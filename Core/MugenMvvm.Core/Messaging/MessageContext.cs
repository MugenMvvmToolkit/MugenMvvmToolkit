using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Metadata;

namespace MugenMvvm.Messaging
{
    public sealed class MessageContext : MetadataOwnerBase, IMessageContext
    {
        #region Constructors

        public MessageContext(object? sender, object message, IReadOnlyMetadataContext? metadata = null, IMetadataContextProvider? metadataContextProvider = null)
            : base(metadata, metadataContextProvider)
        {
            Should.NotBeNull(message, nameof(message));
            Sender = sender;
            Message = message;
        }

        #endregion

        #region Properties

        public object? Sender { get; }

        public object Message { get; }

        #endregion
    }
}