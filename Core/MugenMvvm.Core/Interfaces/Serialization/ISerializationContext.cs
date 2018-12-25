using System;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Serialization
{
    public interface ISerializationContext
    {
        IServiceProvider ServiceProvider { get; }

        ISerializer Serializer { get; }

        IMetadataContext Metadata { get; }
    }
}