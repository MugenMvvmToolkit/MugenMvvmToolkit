using System;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Metadata;

namespace MugenMvvm.Serialization
{
    public sealed class SerializationContext<TRequest, TResult> : MetadataOwnerBase, ISerializationContext
    {
        public SerializationContext(ISerializationFormatBase<TRequest, TResult> format, TRequest request, IReadOnlyMetadataContext? metadata = null)
            : base(metadata)
        {
            Should.NotBeNull(format, nameof(format));
            Format = format;
            Request = request;
        }

        public ISerializationFormatBase<TRequest, TResult> Format { get; }

        public TRequest Request { get; }

        public Type RequestType => typeof(TRequest);

        public Type ResultType => typeof(TResult);

        ISerializationFormatBase ISerializationContext.Format => Format;

        object ISerializationContext.Request => Request!;

        public void Dispose() => this.ClearMetadata(true);
    }
}