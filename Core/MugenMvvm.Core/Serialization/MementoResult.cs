using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Serialization;

namespace MugenMvvm.Serialization
{
    public class MementoResult : IMementoResult
    {
        #region Fields

        public static readonly IMementoResult Unrestored = new MementoResult();

        #endregion

        #region Constructors

        private MementoResult()
        {
            Metadata = Default.Metadata;
        }

        public MementoResult(object? target, IMetadataOwner<IReadOnlyMetadataContext>? metadataOwner = null)
            : this(target, metadataOwner?.Metadata)
        {
        }

        public MementoResult(object? target, IReadOnlyMetadataContext? metadata = null)
        {
            IsRestored = true;
            Metadata = metadata.DefaultIfNull();
            Target = target;
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