using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Serialization
{
    public interface ISerializationContext : IHasMetadata<IMetadataContext> //todo review?
    {
        IServiceProvider ServiceProvider { get; }

        ISerializer Serializer { get; }
    }
}