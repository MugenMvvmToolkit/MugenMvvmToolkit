using System;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Serialization
{
    public interface ISerializationContext : IMetadataOwner<IMetadataContext> //todo review?
    {
        IServiceProvider ServiceProvider { get; }

        ISerializer Serializer { get; }
    }
}