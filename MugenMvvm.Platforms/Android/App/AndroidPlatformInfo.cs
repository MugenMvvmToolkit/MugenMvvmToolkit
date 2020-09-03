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

        public override string ApplicationVersion => _appVersion ??= DeviceService.AppVersion();

        public override PlatformIdiom Idiom => _idiom ??= GetIdiom();

        public override string DeviceVersion => _deviceVersion ??= DeviceService.Version();

        #endregion

        #region Methods

        private static PlatformIdiom GetIdiom() =>
            DeviceService.Idiom() switch
            {
                DeviceService.Desktop => PlatformIdiom.Desktop,
                DeviceService.Phone => PlatformIdiom.Phone,
                DeviceService.Tablet => PlatformIdiom.Tablet,
                DeviceService.Tv => PlatformIdiom.TV,
                DeviceService.Watch => PlatformIdiom.Watch,
                _ => PlatformIdiom.Unknown
            };

        #endregion
    }
}