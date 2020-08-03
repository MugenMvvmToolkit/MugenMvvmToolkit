using MugenMvvm.Enums;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Metadata;

namespace MugenMvvm.App
{
    public class DeviceInfo : MetadataOwnerBase, IDeviceInfo
    {
        #region Constructors

        protected internal DeviceInfo(PlatformType platform, IReadOnlyMetadataContext? metadata = null) : base(metadata)
        {
            Should.NotBeNull(platform, nameof(platform));
            Platform = platform;
        }

        #endregion

        #region Properties

        public PlatformType Platform { get; }

        public virtual PlatformIdiom Idiom => PlatformIdiom.Unknown;

        public virtual string Version => "0.0";

        #endregion
    }
}