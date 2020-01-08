using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Messaging
{
    public sealed class MessageContext : IMessageContext
    {
        #region Fields

        private readonly IMetadataContextProvider? _metadataContextProvider;
        private IReadOnlyMetadataContext? _metadata;

        #endregion

        #region Constructors

        public MessageContext(object? sender, object message, IReadOnlyMetadataContext? metadata, IMetadataContextProvider? metadataContextProvider = null)
        {
            _metadata = metadata;
            _metadataContextProvider = metadataContextProvider;
            Sender = sender;
            Message = message;
        }

        #endregion

        #region Properties

        public bool HasMetadata => !_metadata.IsNullOrEmpty();

        public IMetadataContext Metadata
        {
            get
            {
                if (_metadata is IMetadataContext ctx)
                    return ctx;
                return _metadataContextProvider.LazyInitializeNonReadonly(ref _metadata, this);
            }
        }

        public object? Sender { get; }

        public object Message { get; }

        #endregion
    }
}