using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Interfaces.Metadata;
#if SPAN_API
using System;

#endif

namespace MugenMvvm.Enums
{
    public abstract class DeserializationFormat : DeserializationFormat<object, object>
    {
        #region Fields

#if SPAN_API
        public static readonly IDeserializationFormat<ReadOnlyMemory<byte>, object?> JsonBytes = new DeserializationFormat<ReadOnlyMemory<byte>, object?>(1, InternalConstant.JsonFormat);
        public static readonly IDeserializationFormat<ReadOnlyMemory<byte>, IReadOnlyMetadataContext?> AppStateBytes = new DeserializationFormat<ReadOnlyMemory<byte>, IReadOnlyMetadataContext?>(-1, InternalConstant.StateFormat);
#else
        public static readonly IDeserializationFormat<byte[], object?> JsonBytes = new DeserializationFormat<byte[], object?>(1, InternalConstant.JsonFormat);
        public static readonly IDeserializationFormat<byte[], IReadOnlyMetadataContext?> AppStateBytes = new DeserializationFormat<byte[], IReadOnlyMetadataContext?>(-1, InternalConstant.StateFormat);
#endif

        #endregion

        #region Constructors

        protected DeserializationFormat()
        {
        }

        #endregion
    }
}