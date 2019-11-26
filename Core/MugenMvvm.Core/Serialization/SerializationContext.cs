using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Serialization;

namespace MugenMvvm.Serialization
{
    public sealed class SerializationContext : ISerializationContext
    {
        #region Constructors

        public SerializationContext(ISerializer serializer, IMetadataContext metadata)
        {
            Should.NotBeNull(serializer, nameof(serializer));
            Should.NotBeNull(metadata, nameof(metadata));
            Metadata = metadata;
            Serializer = serializer;
        }

        #endregion

        #region Properties

        [field: ThreadStatic]
        public static ISerializationContext? CurrentSerializationContext { get; private set; }

        public bool HasMetadata => Metadata.Count != 0;

        public IMetadataContext Metadata { get; }

        public ISerializer Serializer { get; }

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