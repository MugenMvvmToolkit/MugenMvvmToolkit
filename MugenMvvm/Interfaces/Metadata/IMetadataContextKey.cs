using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Metadata
{
    public interface IMetadataContextKey : IEquatable<IMetadataContextKey?>, IHasId<string>
    {
        bool IsSerializable { get; }

        Type ValueType { get; }

        IReadOnlyDictionary<string, object?> Metadata { get; }
    }
}