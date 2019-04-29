using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Views.Infrastructure;

namespace MugenMvvm.Infrastructure.Views
{
    public class CleanupViewManagerResult : ICleanupViewManagerResult
    {
        #region Fields

        public static readonly ICleanupViewManagerResult Empty = new CleanupViewManagerResult(Default.MetadataContext);

        #endregion

        #region Constructors

        public CleanupViewManagerResult(IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(metadata, nameof(metadata));
            Metadata = metadata;
        }

        #endregion

        #region Properties

        public bool IsMetadataInitialized => true;

        public IReadOnlyMetadataContext Metadata { get; }

        #endregion
    }
}