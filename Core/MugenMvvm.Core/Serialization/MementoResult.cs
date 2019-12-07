using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Serialization;

namespace MugenMvvm.Serialization
{
    public sealed class MementoResult : IMementoResult
    {
        #region Fields

        public static readonly IMementoResult Unrestored = new MementoResult(null);

        #endregion

        #region Constructors

        public MementoResult(IReadOnlyMetadataContext? metadata)
        {
            Metadata = metadata.DefaultIfNull();
        }

        public MementoResult(object target, IMetadataOwner<IReadOnlyMetadataContext>? metadataOwner = null)
            : this(target, metadataOwner?.Metadata)
        {
        }

        public MementoResult(object target, IReadOnlyMetadataContext? metadata = null) : this(metadata)
        {
            Should.NotBeNull(target, nameof(target));
            Target = target;
            IsRestored = true;
        }

        #endregion

        #region Properties

        public bool IsRestored { get; }

        public bool HasMetadata => Metadata.Count != 0;

        public IReadOnlyMetadataContext Metadata { get; }

        public object? Target { get; }

        #endregion
    }
}