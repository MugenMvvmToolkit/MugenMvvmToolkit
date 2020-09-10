using System;
using System.IO;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Serialization
{
    public interface ISerializationContext : IMetadataOwner<IMetadataContext>, IDisposable
    {
        bool IsSerialization { get; }

        Stream Stream { get; }
    }
}