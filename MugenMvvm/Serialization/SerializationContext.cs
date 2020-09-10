using System.IO;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Metadata;

namespace MugenMvvm.Serialization
{
    public sealed class SerializationContext : MetadataOwnerBase, ISerializationContext
    {
        #region Constructors

        public SerializationContext(Stream stream, bool isSerialization, IReadOnlyMetadataContext? metadata = null)
            : base(metadata)
        {
            Should.NotBeNull(stream, nameof(stream));
            Stream = stream;
            IsSerialization = isSerialization;
        }

        #endregion

        #region Properties

        public bool IsSerialization { get; }

        public Stream Stream { get; }

        #endregion

        #region Implementation of interfaces

        public void Dispose() => this.ClearMetadata(true);

        #endregion
    }
}