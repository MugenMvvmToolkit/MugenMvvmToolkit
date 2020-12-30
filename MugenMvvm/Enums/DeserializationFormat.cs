using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Serialization;
#if SPAN_API
using System;

#endif

namespace MugenMvvm.Enums
{
    public abstract class DeserializationFormat : DeserializationFormat<object, object>
    {
        #region Fields

#if SPAN_API
        public static readonly IDeserializationFormat<ReadOnlyMemory<byte>, object?> JsonBytes = new DeserializationFormat<ReadOnlyMemory<byte>, object?>(0, SerializationFormat.JsonName);
        public static readonly IDeserializationFormat<ReadOnlyMemory<byte>, IReadOnlyMetadataContext?> AppStateBytes =
            new DeserializationFormat<ReadOnlyMemory<byte>, IReadOnlyMetadataContext?>(-1, SerializationFormat.AppStateName);
#else
        public static readonly IDeserializationFormat<byte[], object?> JsonBytes = new DeserializationFormat<byte[], object?>(0, SerializationFormat.JsonName);
        public static readonly IDeserializationFormat<byte[], IReadOnlyMetadataContext?> AppStateBytes =
            new DeserializationFormat<byte[], IReadOnlyMetadataContext?>(-1, SerializationFormat.AppStateName);
#endif

        #endregion

        #region Constructors

        #endregion
    }
}