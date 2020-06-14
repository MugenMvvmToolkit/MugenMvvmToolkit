using MugenMvvm.Enums;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.App
{
    public sealed class DeviceInfo : IDeviceInfo
    {
        #region Constructors

        public DeviceInfo(PlatformType platform, PlatformIdiom idiom, string rawVersion, IMetadataContext metadata)
        {
            Should.NotBeNull(platform, nameof(platform));
            Should.NotBeNull(idiom, nameof(idiom));
            Should.NotBeNull(rawVersion, nameof(rawVersion));
            Should.NotBeNull(metadata, nameof(metadata));
            Platform = platform;
            Idiom = idiom;
            RawVersion = rawVersion;
            Metadata = metadata;
        }

        #endregion

        #region Properties

        public PlatformType Platform { get; }

        public PlatformIdiom Idiom { get; }

        public string RawVersion { get; }

        public bool HasMetadata => Metadata.Count != 0;

        public IMetadataContext Metadata { get; }

        #endregion
    }
}