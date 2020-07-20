using MugenMvvm.Enums;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Metadata;

namespace MugenMvvm.App
{
    public sealed class DeviceInfo : MetadataOwnerBase, IDeviceInfo
    {
        #region Constructors

        public DeviceInfo(PlatformType platform, PlatformIdiom idiom, string rawVersion, IReadOnlyMetadataContext? metadata = null) : base(metadata)
        {
            Should.NotBeNull(platform, nameof(platform));
            Should.NotBeNull(idiom, nameof(idiom));
            Should.NotBeNull(rawVersion, nameof(rawVersion));
            Platform = platform;
            Idiom = idiom;
            RawVersion = rawVersion;
        }

        #endregion

        #region Properties

        public PlatformType Platform { get; }

        public PlatformIdiom Idiom { get; }

        public string RawVersion { get; }

        #endregion
    }
}