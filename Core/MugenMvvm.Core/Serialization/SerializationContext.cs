using System;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Serialization;

namespace MugenMvvm.Serialization
{
    public sealed class SerializationContext : ISerializationContext
    {
        #region Fields

        private readonly IMetadataContextProvider? _metadataContextProvider;
        private IReadOnlyMetadataContext? _metadata;

        #endregion

        #region Constructors

        public SerializationContext(IReadOnlyMetadataContext? metadata, IMetadataContextProvider? metadataContextProvider = null)
        {
            _metadata = metadata;
            _metadataContextProvider = metadataContextProvider;
        }

        #endregion

        #region Properties

        [field: ThreadStatic]
        public static ISerializationContext? CurrentSerializationContext { get; private set; }

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

        #endregion

        #region Methods

        public static ActionToken Begin(ISerializationContext context)
        {
            Should.NotBeNull(context, nameof(context));
            CurrentSerializationContext = context;
            return new ActionToken((_, __) => CurrentSerializationContext = null);
        }

        #endregion
    }
}