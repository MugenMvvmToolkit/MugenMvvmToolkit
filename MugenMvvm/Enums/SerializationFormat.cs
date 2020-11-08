using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Interfaces.Metadata;
#if SPAN_API
using System;

#endif

namespace MugenMvvm.Enums
{
    public abstract class SerializationFormat : SerializationFormat<object, object>
    {
        #region Fields

#if SPAN_API
        public static readonly ISerializationFormat<object, ReadOnlyMemory<byte>> JsonBytes = new SerializationFormat<object, ReadOnlyMemory<byte>>(1, InternalConstant.JsonFormat);
        public static readonly ISerializationFormat<IReadOnlyMetadataContext, ReadOnlyMemory<byte>> AppStateBytes = new SerializationFormat<IReadOnlyMetadataContext, ReadOnlyMemory<byte>>(-1, InternalConstant.StateFormat);
#else
        public static readonly ISerializationFormat<object, byte[]> JsonBytes = new SerializationFormat<object, byte[]>(1, InternalConstant.JsonFormat);
        public static readonly ISerializationFormat<IReadOnlyMetadataContext, byte[]> AppStateBytes = new SerializationFormat<IReadOnlyMetadataContext, byte[]>(-1, InternalConstant.StateFormat);
#endif

        #endregion

        #region Constructors

        protected SerializationFormat()
        {
        }

        #endregion
    }
}