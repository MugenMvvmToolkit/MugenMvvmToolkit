using System;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Metadata;

namespace MugenMvvm.Serialization
{
    public sealed class SerializationContext<TRequest, TResult> : MetadataOwnerBase, ISerializationContext
    {
        #region Constructors

        public SerializationContext(ISerializationFormatBase<TRequest, TResult> format, TRequest request, IReadOnlyMetadataContext? metadata = null)
            : base(metadata)
        {
            Should.NotBeNull(format, nameof(format));
            Format = format;
            Request = request;
        }

        #endregion

        #region Properties

        ISerializationFormatBase ISerializationContext.Format => Format;

        object ISerializationContext.Request => Request!;

        public Type RequestType => typeof(TRequest);

        public Type ResultType => typeof(TResult);

        public ISerializationFormatBase<TRequest, TResult> Format { get; }

        public TRequest Request { get; }

        #endregion

        #region Implementation of interfaces

        public void Dispose() => this.ClearMetadata(true);

        #endregion
    }
}