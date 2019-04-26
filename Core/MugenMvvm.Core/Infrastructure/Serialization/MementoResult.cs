using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Serialization;

namespace MugenMvvm.Infrastructure.Serialization
{
    public class MementoResult : IMementoResult
    {
        #region Fields

        public static readonly IMementoResult Unrestored;

        #endregion

        #region Constructors

        static MementoResult()
        {
            Unrestored = new MementoResult();
        }

        private MementoResult()
        {
            Metadata = Default.MetadataContext;
        }

        public MementoResult(object? target, IHasMetadata<IReadOnlyMetadataContext>? hasMetadata = null)
            : this(target, hasMetadata?.Metadata.DefaultIfNull())
        {
        }

        public MementoResult(object? target, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(metadata, nameof(metadata));
            IsRestored = true;
            Metadata = metadata;
            Target = target;
        }

        #endregion

        #region Properties

        public bool IsRestored { get; }

        public IReadOnlyMetadataContext Metadata { get; }

        public object? Target { get; }

        #endregion
    }
}