using System;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Serialization
{
    public interface ISerializationContext : IMetadataOwner<IMetadataContext>
    {
        ISerializationFormatBase Format { get; }

        Type RequestType { get; }

        Type ResultType { get; }

        object Request { get; }
    }
}