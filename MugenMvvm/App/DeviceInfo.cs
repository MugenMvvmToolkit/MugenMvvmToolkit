using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Metadata;

namespace MugenMvvm.App
{
    public sealed class DeviceInfo : MetadataOwnerBase, IDeviceInfo
    {
        #region Fields

        private readonly Func<PlatformIdiom> _getIdiom;
        private readonly Func<string> _getRawVersion;
        private PlatformIdiom? _idiom;
        private string? _rawVersion;

        #endregion

        #region Constructors

        public DeviceInfo(PlatformType platform, Func<PlatformIdiom> getIdiom, Func<string> getRawVersion, IReadOnlyMetadataContext? metadata = null) : base(metadata)
        {
            Should.NotBeNull(platform, nameof(platform));
            Should.NotBeNull(getIdiom, nameof(getIdiom));
            Should.NotBeNull(getRawVersion, nameof(getRawVersion));
            _getIdiom = getIdiom;
            _getRawVersion = getRawVersion;
            Platform = platform;
        }

        #endregion

        #region Properties

        public PlatformType Platform { get; }

        public PlatformIdiom Idiom
        {
            get
            {
                if (_idiom == null)
                    _idiom = _getIdiom();
                return _idiom;
            }
        }

        public string RawVersion
        {
            get
            {
                if (_rawVersion == null)
                    _rawVersion = _getRawVersion();
                return _rawVersion;
            }
        }

        #endregion
    }
}