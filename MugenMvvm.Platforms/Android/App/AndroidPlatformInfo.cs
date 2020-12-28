using MugenMvvm.Android.Native;
using MugenMvvm.App;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Android.App
{
    public class AndroidPlatformInfo : PlatformInfo
    {
        #region Fields

        private string? _appVersion;
        private string? _deviceVersion;

        private PlatformIdiom? _idiom;

        #endregion

        #region Constructors

        public AndroidPlatformInfo(IReadOnlyMetadataContext? metadata = null) : base(PlatformType.Android, metadata)
        {
        }

        #endregion

        #region Properties

        public override string ApplicationVersion => _appVersion ??= MugenAndroidUtils.AppVersion();

        public override PlatformIdiom Idiom => _idiom ??= GetIdiom();

        public override string DeviceVersion => _deviceVersion ??= MugenAndroidUtils.Version();

        #endregion

        #region Methods

        private static PlatformIdiom GetIdiom() =>
            MugenAndroidUtils.Idiom() switch
            {
                MugenAndroidUtils.Desktop => PlatformIdiom.Desktop,
                MugenAndroidUtils.Phone => PlatformIdiom.Phone,
                MugenAndroidUtils.Tablet => PlatformIdiom.Tablet,
                MugenAndroidUtils.Tv => PlatformIdiom.TV,
                MugenAndroidUtils.Watch => PlatformIdiom.Watch,
                _ => PlatformIdiom.Unknown
            };

        #endregion
    }
}