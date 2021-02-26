using MugenMvvm.Android.Native;
using MugenMvvm.App;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Android.App
{
    public class AndroidPlatformInfo : PlatformInfo
    {
        public AndroidPlatformInfo(IReadOnlyMetadataContext? metadata = null) : base(PlatformType.Android, null, null, null, metadata)
        {
        }

        protected override string GetAppVersion() => MugenAndroidUtils.AppVersion();

        protected override string GetDeviceVersion() => MugenAndroidUtils.Version();

        protected override PlatformIdiom GetIdiom() =>
            MugenAndroidUtils.Idiom() switch
            {
                MugenAndroidUtils.Desktop => PlatformIdiom.Desktop,
                MugenAndroidUtils.Phone => PlatformIdiom.Phone,
                MugenAndroidUtils.Tablet => PlatformIdiom.Tablet,
                MugenAndroidUtils.Tv => PlatformIdiom.TV,
                MugenAndroidUtils.Watch => PlatformIdiom.Watch,
                _ => PlatformIdiom.Unknown
            };
    }
}