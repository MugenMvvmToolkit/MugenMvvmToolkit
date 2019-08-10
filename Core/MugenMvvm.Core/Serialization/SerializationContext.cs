using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Serialization;

namespace MugenMvvm.Serialization
{
    public class SerializationContext : ISerializationContext
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

        public bool HasMetadata => true;

        public IMetadataContext Metadata { get; }

        public ISerializer Serializer { get; }

        #endregion
    }
}