using MugenMvvm.Android.Native;
using MugenMvvm.App;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Android.App
{
    public class AndroidDeviceInfo : DeviceInfo
    {
        #region Fields

        private PlatformIdiom? _idiom;
        private string? _version;

        #endregion

        #region Constructors

        public AndroidDeviceInfo(IReadOnlyMetadataContext? metadata = null) : base(PlatformType.Android, metadata)
        {
        }

        #endregion

        #region Properties

        public override PlatformIdiom Idiom => _idiom ??= GetIdiom();

        public override string Version => _version ??= DeviceService.Version();

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