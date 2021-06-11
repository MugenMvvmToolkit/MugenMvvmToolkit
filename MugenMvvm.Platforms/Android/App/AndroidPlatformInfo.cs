using MugenMvvm.Android.Native;
using MugenMvvm.Android.Native.Constants;
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
                IdiomType.Desktop => PlatformIdiom.Desktop,
                IdiomType.Phone => PlatformIdiom.Phone,
                IdiomType.Tablet => PlatformIdiom.Tablet,
                IdiomType.Tv => PlatformIdiom.TV,
                IdiomType.Watch => PlatformIdiom.Watch,
                _ => PlatformIdiom.Unknown
            };
    }
}