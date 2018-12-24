using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Models;

namespace MugenMvvm.Interfaces.Serialization
{
    public interface ISerializationContext
    {
        SerializationMode Mode { get; }

        IServiceProvider ServiceProvider { get; }

        ISerializer Serializer { get; }

        IMetadataContext Metadata { get; }
    }
}