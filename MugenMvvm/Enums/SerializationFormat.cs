using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Serialization;
#if SPAN_API
using System;

#endif

namespace MugenMvvm.Enums
{
    public abstract class SerializationFormat : SerializationFormat<object, object>
    {
        #region Fields

        public const string JsonName = "json";
        public const string AppStateName = "state";

#if SPAN_API
        public static readonly ISerializationFormat<object, ReadOnlyMemory<byte>> JsonBytes = new SerializationFormat<object, ReadOnlyMemory<byte>>(0, JsonName);
        public static readonly ISerializationFormat<IReadOnlyMetadataContext, ReadOnlyMemory<byte>> AppStateBytes =
            new SerializationFormat<IReadOnlyMetadataContext, ReadOnlyMemory<byte>>(-1, AppStateName);
#else
        public static readonly ISerializationFormat<object, byte[]> JsonBytes = new SerializationFormat<object, byte[]>(0, JsonName);
        public static readonly ISerializationFormat<IReadOnlyMetadataContext, byte[]> AppStateBytes =
            new SerializationFormat<IReadOnlyMetadataContext, byte[]>(-1, AppStateName);
#endif

        #endregion

        #region Constructors

        #endregion
    }
}