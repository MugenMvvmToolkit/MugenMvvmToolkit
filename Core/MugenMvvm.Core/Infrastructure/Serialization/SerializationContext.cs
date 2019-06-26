using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Serialization;

namespace MugenMvvm.Infrastructure.Serialization
{
    public class SerializationContext : ISerializationContext
    {
        #region Constructors

        public SerializationContext(ISerializer serializer, IServiceProvider serviceProvider, IMetadataContext metadata)
        {
            Should.NotBeNull(serializer, nameof(serializer));
            Should.NotBeNull(serviceProvider, nameof(serviceProvider));
            Should.NotBeNull(metadata, nameof(metadata));
            Metadata = metadata;
            ServiceProvider = serviceProvider;
            Serializer = serializer;
        }

        #endregion

        #region Properties

        public bool HasMetadata => true;

        public IMetadataContext Metadata { get; }

        public IServiceProvider ServiceProvider { get; }

        public ISerializer Serializer { get; }

        #endregion
    }
}