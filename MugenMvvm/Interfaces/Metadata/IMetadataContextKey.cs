using System;
using System.Collections.Generic;

namespace MugenMvvm.Interfaces.Metadata
{
    public interface IMetadataContextKey : IEquatable<IMetadataContextKey?>
    {
        bool IsSerializable { get; }

        IReadOnlyDictionary<string, object?> Metadata { get; }
    }
}